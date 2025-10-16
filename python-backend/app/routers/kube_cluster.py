"""
PodMD Python Backend - Kube Cluster Router
Kubernetes cluster management endpoints.
"""

import uuid
from typing import List

from fastapi import APIRouter, Depends, HTTPException, status
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy.future import select
from sqlalchemy.orm import selectinload

from app.database import get_db
from app.models.user import User
from app.models.source import Source
from app.models.kube_cluster import KubeCluster
from app.schemas.kube_cluster import (
    KubeClusterCreate,
    KubeClusterUpdate,
    KubeClusterResponse,
    KubeClusterListResponse
)
from app.auth import get_current_user


router = APIRouter(prefix="/kube-clusters", tags=["kube-clusters"])


@router.post("/", response_model=KubeClusterResponse, status_code=status.HTTP_201_CREATED)
async def create_kube_cluster(
    kube_cluster: KubeClusterCreate,
    db: AsyncSession = Depends(get_db),
    current_user: User = Depends(get_current_user)
):
    """Create a new Kubernetes cluster with source configuration."""
    try:
        # Create Source entity
        source_id = str(uuid.uuid4())
        source = Source(
            id=source_id,
            user_id=current_user.id,
            type="kubernetes",  # Always "kubernetes" for clusters
            name=kube_cluster.name,
            server=str(kube_cluster.server),  # Convert HttpUrl to string
            key_version=1,
            instructions=kube_cluster.instructions,
            response_format=kube_cluster.response_format
        )
        db.add(source)

        # Create KubeCluster entity with same ID
        kube_cluster_entity = KubeCluster(
            id=source_id,
            bearer_token_enc=kube_cluster.bearer_token,  # Store plain token for now
            certificate_authority_pem=kube_cluster.certificate_authority_pem,
            insecure_skip_tls_verify=kube_cluster.insecure_skip_tls_verify,
            default_namespace=kube_cluster.default_namespace
        )
        db.add(kube_cluster_entity)

        await db.commit()  # Commit both entities
        await db.refresh(source)
        await db.refresh(kube_cluster_entity)

        # Build response with combined data
        response = KubeClusterResponse(
            id=source_id,
            type=source.type,
            name=source.name,
            server=source.server,
            key_version=source.key_version,
            instructions=source.instructions,
            response_format=source.response_format,
            user_id=source.user_id,
            created_at=source.created_at,
            updated_at=source.updated_at,
            bearer_token_enc=kube_cluster_entity.bearer_token_enc,
            certificate_authority_pem=kube_cluster_entity.certificate_authority_pem,
            insecure_skip_tls_verify=kube_cluster_entity.insecure_skip_tls_verify,
            default_namespace=kube_cluster_entity.default_namespace
        )
        return response

    except Exception as e:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Failed to create Kubernetes cluster: {str(e)}"
        )


@router.get("/", response_model=KubeClusterListResponse)
async def list_kube_clusters(
    db: AsyncSession = Depends(get_db),
    current_user: User = Depends(get_current_user)
):
    """List all Kubernetes clusters for the current user."""
    try:
        # Join Source and KubeCluster, filter by user and type
        query = select(Source, KubeCluster).join(
            KubeCluster, Source.id == KubeCluster.id
        ).where(
            Source.user_id == current_user.id,
            Source.type == "kubernetes"
        )

        result = await db.execute(query)
        clusters_data = result.all()

        clusters = []
        for source, kube_cluster in clusters_data:
            cluster_response = KubeClusterResponse(
                id=str(source.id),
                type=source.type,
                name=source.name,
                server=source.server,
                key_version=source.key_version,
                instructions=source.instructions,
                response_format=source.response_format,
                user_id=str(source.user_id),
                created_at=source.created_at,
                updated_at=source.updated_at,
                bearer_token_enc=kube_cluster.bearer_token_enc,
                certificate_authority_pem=kube_cluster.certificate_authority_pem,
                insecure_skip_tls_verify=kube_cluster.insecure_skip_tls_verify,
                default_namespace=kube_cluster.default_namespace
            )
            clusters.append(cluster_response)

        return KubeClusterListResponse(kube_clusters=clusters)

    except Exception as e:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Failed to list Kubernetes clusters: {str(e)}"
        )


@router.get("/{cluster_id}", response_model=KubeClusterResponse)
async def get_kube_cluster(
    cluster_id: str,
    db: AsyncSession = Depends(get_db),
    current_user: User = Depends(get_current_user)
):
    """Get a specific Kubernetes cluster by ID."""
    try:
        # Join Source and KubeCluster, filter by user and type
        query = select(Source, KubeCluster).join(
            KubeCluster, Source.id == KubeCluster.id
        ).where(
            Source.id == cluster_id,
            Source.user_id == current_user.id,
            Source.type == "kubernetes"
        )

        result = await db.execute(query)
        cluster_data = result.first()

        if not cluster_data:
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail="Kubernetes cluster not found"
            )

        source, kube_cluster = cluster_data
        return KubeClusterResponse(
            id=str(source.id),
            type=source.type,
            name=source.name,
            server=source.server,
            key_version=source.key_version,
            instructions=source.instructions,
            response_format=source.response_format,
            user_id=str(source.user_id),
            created_at=source.created_at,
            updated_at=source.updated_at,
            bearer_token_enc=kube_cluster.bearer_token_enc,
            certificate_authority_pem=kube_cluster.certificate_authority_pem,
            insecure_skip_tls_verify=kube_cluster.insecure_skip_tls_verify,
            default_namespace=kube_cluster.default_namespace
        )

    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Failed to get Kubernetes cluster: {str(e)}"
        )


@router.put("/{cluster_id}", response_model=KubeClusterResponse)
async def update_kube_cluster(
    cluster_id: str,
    update_data: KubeClusterUpdate,
    db: AsyncSession = Depends(get_db),
    current_user: User = Depends(get_current_user)
):
    """Update an existing Kubernetes cluster."""
    try:
        # Get existing entities
        query = select(Source, KubeCluster).join(
            KubeCluster, Source.id == KubeCluster.id
        ).where(
            Source.id == cluster_id,
            Source.user_id == current_user.id,
            Source.type == "kubernetes"
        )

        result = await db.execute(query)
        cluster_data = result.first()

        if not cluster_data:
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail="Kubernetes cluster not found"
            )

        source, kube_cluster = cluster_data

        # Update Source fields if provided
        if update_data.name is not None:
            source.name = update_data.name
        if update_data.server is not None:
            source.server = str(update_data.server)
        if update_data.key_version is not None:
            source.key_version = update_data.key_version
        if update_data.instructions is not None:
            source.instructions = update_data.instructions
        if update_data.response_format is not None:
            source.response_format = update_data.response_format

        # Update KubeCluster fields if provided
        if update_data.bearer_token is not None:
            kube_cluster.bearer_token_enc = update_data.bearer_token
        if update_data.certificate_authority_pem is not None:
            kube_cluster.certificate_authority_pem = update_data.certificate_authority_pem
        if update_data.insecure_skip_tls_verify is not None:
            kube_cluster.insecure_skip_tls_verify = update_data.insecure_skip_tls_verify
        if update_data.default_namespace is not None:
            kube_cluster.default_namespace = update_data.default_namespace

        await db.commit()
        await db.refresh(source)
        await db.refresh(kube_cluster)

        return KubeClusterResponse(
            id=str(source.id),
            type=source.type,
            name=source.name,
            server=source.server,
            key_version=source.key_version,
            instructions=source.instructions,
            response_format=source.response_format,
            user_id=str(source.user_id),
            created_at=source.created_at,
            updated_at=source.updated_at,
            bearer_token_enc=kube_cluster.bearer_token_enc,
            certificate_authority_pem=kube_cluster.certificate_authority_pem,
            insecure_skip_tls_verify=kube_cluster.insecure_skip_tls_verify,
            default_namespace=kube_cluster.default_namespace
        )

    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Failed to update Kubernetes cluster: {str(e)}"
        )


@router.delete("/{cluster_id}", status_code=status.HTTP_204_NO_CONTENT)
async def delete_kube_cluster(
    cluster_id: str,
    db: AsyncSession = Depends(get_db),
    current_user: User = Depends(get_current_user)
):
    """Delete a Kubernetes cluster."""
    try:
        # Verify cluster exists and belongs to user
        query = select(Source).join(
            KubeCluster, Source.id == KubeCluster.id
        ).where(
            Source.id == cluster_id,
            Source.user_id == current_user.id,
            Source.type == "kubernetes"
        )

        result = await db.execute(query)
        source = result.scalar_one_or_none()

        if not source:
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail="Kubernetes cluster not found"
            )

        # Delete (KubeCluster will be deleted cascade due to FK constraint)
        await db.delete(source)
        await db.commit()

    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Failed to delete Kubernetes cluster: {str(e)}"
        )
