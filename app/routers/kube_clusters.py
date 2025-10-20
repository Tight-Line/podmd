"""
Kubernetes clusters CRUD router.

Provides REST API endpoints for managing Kubernetes cluster configurations
with JWT authentication, encrypted credential storage, and complete error handling.
"""

from uuid import UUID
from typing import List

from fastapi import APIRouter, Depends, HTTPException, status
from fastapi.security import OAuth2PasswordBearer
from jose import jwt, JWTError
from sqlalchemy.ext.asyncio import AsyncSession

from app.config import settings
from app.database import get_db
from app.models import User
from app.schemas.kube_clusters import (
    CreateKubeClusterRequest,
    UpdateKubeClusterRequest,
    KubeClusterResponse,
    KubeClusterListResponse,
)
from app.services.kube_clusters import cluster_service
from sqlalchemy import select


# JWT OAuth2 scheme
oauth2_scheme = OAuth2PasswordBearer(tokenUrl="/auth/login")


async def get_current_user(
    token: str = Depends(oauth2_scheme),
    session: AsyncSession = Depends(get_db)
) -> User:
    """
    Dependency to get the current authenticated user from JWT token.

    Validates the token and fetches the user from the database.
    """
    credentials_exception = HTTPException(
        status_code=status.HTTP_401_UNAUTHORIZED,
        detail="Could not validate credentials",
        headers={"WWW-Authenticate": "Bearer"},
    )
    try:
        payload = jwt.decode(token, settings.jwt_secret_key, algorithms=["HS256"])
        email: str = payload.get("sub")
        if email is None:
            raise credentials_exception
    except JWTError:
        raise credentials_exception

    stmt = select(User).where(User.email == email)
    result = await session.execute(stmt)
    user = result.scalar_one_or_none()

    if user is None:
        raise credentials_exception
    return user


router = APIRouter(
    prefix="/kube-clusters",
    tags=["kube-clusters"],
    dependencies=[Depends(get_current_user)],  # Require auth for all endpoints
)


@router.post(
    "/",
    response_model=KubeClusterResponse,
    status_code=status.HTTP_201_CREATED,
    summary="Create a new Kubernetes cluster",
)
async def create_cluster(
    cluster_data: CreateKubeClusterRequest,
    session: AsyncSession = Depends(get_db),
    current_user: User = Depends(get_current_user),
) -> KubeClusterResponse:
    """
    Create a new Kubernetes cluster for the authenticated user.

    Automatically encrypts the bearer token for secure storage.

    Raises:
        HTTPException: 400 for validation errors
        HTTPException: 401 for authentication issues
        HTTPException: 500 for server errors
    """
    try:
        cluster = await cluster_service.create_cluster(session, current_user.id, cluster_data)
        return cluster
    except Exception as e:
        # Log the error in production
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Failed to create cluster"
        ) from e


@router.get(
    "/",
    response_model=KubeClusterListResponse,
    summary="List user's Kubernetes clusters",
)
async def list_clusters(
    session: AsyncSession = Depends(get_db),
    current_user: User = Depends(get_current_user),
) -> KubeClusterListResponse:
    """
    Retrieve all Kubernetes clusters belonging to the authenticated user.
    """
    try:
        clusters = await cluster_service.get_clusters(session, current_user.id)
        return KubeClusterListResponse(kube_clusters=clusters)
    except Exception as e:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Failed to retrieve clusters"
        ) from e


@router.get(
    "/{cluster_id}",
    response_model=KubeClusterResponse,
    summary="Get specific Kubernetes cluster",
)
async def get_cluster(
    cluster_id: UUID,
    session: AsyncSession = Depends(get_db),
    current_user: User = Depends(get_current_user),
) -> KubeClusterResponse:
    """
    Retrieve a specific Kubernetes cluster by ID.

    Only accessible by the cluster owner.

    Raises:
        HTTPException: 404 if cluster not found or not owned by user
        HTTPException: 401 for authentication issues
    """
    cluster = await cluster_service.get_cluster(session, current_user.id, cluster_id)
    if cluster is None:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Kubernetes cluster not found"
        )
    return cluster


@router.put(
    "/{cluster_id}",
    response_model=KubeClusterResponse,
    summary="Update Kubernetes cluster",
)
async def update_cluster(
    cluster_id: UUID,
    update_data: UpdateKubeClusterRequest,
    session: AsyncSession = Depends(get_db),
    current_user: User = Depends(get_current_user),
) -> KubeClusterResponse:
    """
    Update an existing Kubernetes cluster.

    Only the cluster owner can update. Fields not provided remain unchanged.
    Bearer token is re-encrypted if updated.

    Raises:
        HTTPException: 404 if cluster not found or not owned by user
        HTTPException: 400 for validation errors
        HTTPException: 401 for authentication issues
    """
    cluster = await cluster_service.update_cluster(session, current_user.id, cluster_id, update_data)
    if cluster is None:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Kubernetes cluster not found"
        )
    return cluster


@router.delete(
    "/{cluster_id}",
    status_code=status.HTTP_204_NO_CONTENT,
    summary="Delete Kubernetes cluster",
)
async def delete_cluster(
    cluster_id: UUID,
    session: AsyncSession = Depends(get_db),
    current_user: User = Depends(get_current_user),
) -> None:
    """
    Delete a Kubernetes cluster.

    Only the cluster owner can delete.

    Raises:
        HTTPException: 404 if cluster not found or not owned by user
        HTTPException: 401 for authentication issues
    """
    success = await cluster_service.delete_cluster(session, current_user.id, cluster_id)
    if not success:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Kubernetes cluster not found"
        )
