"""
PodMD Python Backend - Logs Router
Kubernetes log retrieval and analysis endpoints.
"""

from fastapi import APIRouter, Depends, HTTPException, status
import os
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select

from app.database import get_db
from app.auth import get_current_user
from app.models.user import User
from app.services.kube_log_service import KubeLogService
from app.services.llm_client import LLMClient
from app.services.analysis_service import AnalysisService
from app.schemas.log import (
    PodLogRequest, DeploymentLogRequest, LogResponse, LogResult
)
from app.schemas.analysis import AnalysisResponse

router = APIRouter(prefix="/kube-clusters/{cluster_id}/logs", tags=["kube-cluster-logs"])
analysis_router = APIRouter(prefix="/kube-clusters/{cluster_id}/analyze", tags=["kube-cluster-analysis"])


# Dependency injection for log service
def get_log_service() -> KubeLogService:
    # Import here to check if kubernetes is available during dependency injection
    from app.services.kube_log_service import KUBERNETES_AVAILABLE, KubeLogService

    if not KUBERNETES_AVAILABLE:
        raise RuntimeError("Kubernetes dependencies not available. Please check kubernetes-asyncio installation.")
    return KubeLogService()


@router.post("/pods", response_model=LogResponse, status_code=status.HTTP_200_OK)
async def get_pod_logs(
    cluster_id: str,
    request: PodLogRequest,
    db: AsyncSession = Depends(get_db),
    current_user: User = Depends(get_current_user),
    log_service: KubeLogService = Depends(get_log_service)
):
    """
    Retrieve logs from a specific pod.

    Requires authentication and cluster ownership.
    """
    try:
        result = await log_service.get_pod_logs_async(cluster_id, request, db)
        return LogResponse.from_log_result(result)

    except ValueError as e:
        # Cluster not found or invalid request
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND if "not found" in str(e) else status.HTTP_400_BAD_REQUEST,
            detail=str(e)
        )
    except Exception as e:
        # Kubernetes API errors
        raise HTTPException(
            status_code=status.HTTP_502_BAD_GATEWAY,
            detail=f"Failed to retrieve logs from Kubernetes API: {str(e)}"
        )


# Dependency injection for analysis service
def get_llm_client():
    api_key = os.getenv("OPENAI_API_KEY")
    if not api_key:
        raise RuntimeError("OPENAI_API_KEY environment variable is not set")
    return LLMClient(api_key=api_key)


def get_analysis_service(
    llm_client: LLMClient = Depends(get_llm_client)
) -> AnalysisService:
    return AnalysisService(llm_client, get_log_service())


@analysis_router.post("/pods", response_model=AnalysisResponse, status_code=status.HTTP_200_OK)
async def analyze_pod_logs(
    cluster_id: str,
    request: PodLogRequest,
    db: AsyncSession = Depends(get_db),
    current_user: User = Depends(get_current_user),
    analysis_service: AnalysisService = Depends(get_analysis_service)
):
    """
    Analyze logs from a specific pod using LLM.

    Retrieves logs from the pod and uses OpenAI to identify errors and provide solutions.
    Requires authentication and cluster ownership.
    """
    try:
        # Get the source for custom instructions
        from app.models.kube_cluster import KubeCluster
        from sqlalchemy.orm import selectinload

        # Query cluster with source relationship using correct SQLAlchemy 2.0 syntax
        stmt = select(KubeCluster).options(selectinload(KubeCluster.source)).where(
            KubeCluster.id == cluster_id,
            KubeCluster.source.has(user_id=current_user.id)
        )
        result = await db.execute(stmt)
        cluster = result.scalar_one_or_none()

        if not cluster or not cluster.source:
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail="Cluster not found or access denied"
            )

        result = await analysis_service.analyze_pod_logs_async(
            cluster_id, request, db, cluster.source
        )
        return result

    except HTTPException:
        raise
    except Exception as e:
        # LLM or other errors
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Analysis failed: {str(e)}"
        )


@analysis_router.post("/deployments", response_model=AnalysisResponse, status_code=status.HTTP_200_OK)
async def analyze_deployment_logs(
    cluster_id: str,
    request: DeploymentLogRequest,
    db: AsyncSession = Depends(get_db),
    current_user: User = Depends(get_current_user),
    analysis_service: AnalysisService = Depends(get_analysis_service)
):
    """
    Analyze logs from failed pods in a deployment using LLM.

    Finds failed pods, retrieves their logs, and uses OpenAI to identify errors and provide solutions.
    Requires authentication and cluster ownership.
    """
    try:
        # Get the source for custom instructions
        from app.models.kube_cluster import KubeCluster
        from sqlalchemy.orm import selectinload

        # Query cluster with source relationship using SQLAlchemy 2.0 syntax
        stmt = select(KubeCluster).options(selectinload(KubeCluster.source)).where(
            KubeCluster.id == cluster_id,
            KubeCluster.source.has(user_id=current_user.id)
        )
        result = await db.execute(stmt)
        cluster = result.scalar_one_or_none()

        if not cluster or not cluster.source:
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail="Cluster not found or access denied"
            )

        result = await analysis_service.analyze_deployment_logs_async(
            cluster_id, request, db, cluster.source
        )
        return result

    except HTTPException:
        raise
    except Exception as e:
        # LLM or other errors
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Analysis failed: {str(e)}"
        )


@router.post("/deployments", response_model=LogResponse, status_code=status.HTTP_200_OK)
async def get_deployment_logs(
    cluster_id: str,
    request: DeploymentLogRequest,
    db: AsyncSession = Depends(get_db),
    current_user: User = Depends(get_current_user),
    log_service: KubeLogService = Depends(get_log_service)
):
    """
    Retrieve logs from failed pods in a deployment.

    Finds failed pods (not Running/Succeeded) in the specified deployment
    and returns logs from the first failed pod. Requires authentication
    and cluster ownership.
    """
    try:
        result = await log_service.get_deployment_logs_async(cluster_id, request, db)
        return LogResponse.from_log_result(result)

    except ValueError as e:
        # Deployment/pod not found or invalid request
        error_msg = str(e).lower()
        if "not found" in error_msg:
            status_code = status.HTTP_404_NOT_FOUND
        elif "no pods found" in error_msg or "no failed pods found" in error_msg:
            status_code = status.HTTP_400_BAD_REQUEST
        else:
            status_code = status.HTTP_400_BAD_REQUEST

        raise HTTPException(
            status_code=status_code,
            detail=str(e)
        )
    except Exception as e:
        # Kubernetes API errors
        raise HTTPException(
            status_code=status.HTTP_502_BAD_GATEWAY,
            detail=f"Failed to retrieve logs from Kubernetes API: {str(e)}"
        )
