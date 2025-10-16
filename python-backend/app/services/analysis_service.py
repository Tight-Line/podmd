"""
PodMD Python Backend - Analysis Service
Orchestrates log retrieval and LLM analysis.
"""

import logging
from typing import Dict, Any
from sqlalchemy.ext.asyncio import AsyncSession

from app.services.llm_client import LLMClient
from app.services.kube_log_service import KubeLogService
from app.models.source import Source
from app.schemas.analysis import AnalysisResponse, AnalysisRequest
from app.schemas.log import DeploymentLogRequest, PodLogRequest

logger = logging.getLogger(__name__)


class AnalysisService:
    """Service for orchestrating Kubernetes log analysis."""

    def __init__(self, llm_client: LLMClient, log_service: KubeLogService):
        self.llm_client = llm_client
        self.log_service = log_service

    async def analyze_pod_logs_async(
        self,
        cluster_id: str,
        request: PodLogRequest,
        db: AsyncSession,
        source: Source
    ) -> AnalysisResponse:
        """Analyze logs from a specific pod."""
        try:
            logger.info(f"Starting pod log analysis for cluster {cluster_id}, pod {request.pod_name}")

            # Get logs from existing service
            log_result = await self.log_service.get_pod_logs_async(cluster_id, request, db)

            if not log_result.logs and not log_result.description:
                logger.warning("No logs or description available for analysis")
                return AnalysisResponse(
                    success=False,
                    message="No logs or pod information available for analysis"
                )

            # Use source instructions or default to built-in prompt
            instructions = source.instructions or None

            # Analyze with LLM
            llm_response = await self.llm_client.analyze_logs_async(
                logs=log_result.logs or "",
                instructions=instructions,
                description=log_result.description
            )

            # Parse response
            parsed_result = self.llm_client.parse_analysis_response(llm_response)

            logger.info("Successfully completed pod log analysis")
            return AnalysisResponse(
                success=True,
                data=parsed_result,
                message="Pod log analysis completed successfully"
            )

        except Exception as e:
            logger.error(f"Pod log analysis failed: {str(e)}")
            return AnalysisResponse(
                success=False,
                message=f"Analysis failed: {str(e)}"
            )

    async def analyze_deployment_logs_async(
        self,
        cluster_id: str,
        request: DeploymentLogRequest,
        db: AsyncSession,
        source: Source
    ) -> AnalysisResponse:
        """Analyze logs from failed pods in a deployment."""
        try:
            logger.info(f"Starting deployment log analysis for cluster {cluster_id}, deployment {request.deployment_name}")

            # Get logs from existing service
            log_result = await self.log_service.get_deployment_logs_async(cluster_id, request, db)

            if not log_result.logs and not log_result.description:
                logger.warning("No logs or description available for analysis")
                return AnalysisResponse(
                    success=False,
                    message="No logs or deployment information available for analysis"
                )

            # Use source instructions or default to built-in prompt
            instructions = source.instructions or None

            # Analyze with LLM
            llm_response = await self.llm_client.analyze_logs_async(
                logs=log_result.logs or "",
                instructions=instructions,
                description=log_result.description
            )

            # Parse response
            parsed_result = self.llm_client.parse_analysis_response(llm_response)

            logger.info("Successfully completed deployment log analysis")
            return AnalysisResponse(
                success=True,
                data=parsed_result,
                message="Deployment log analysis completed successfully"
            )

        except Exception as e:
            logger.error(f"Deployment log analysis failed: {str(e)}")
            return AnalysisResponse(
                success=False,
                message=f"Analysis failed: {str(e)}"
            )
