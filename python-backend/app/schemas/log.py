"""
PodMD Python Backend - Log Schemas
Pydantic schemas for Kubernetes log retrieval requests and responses.
"""

from typing import Optional
from pydantic import BaseModel, Field
from datetime import datetime


class LogMetadata(BaseModel):
    """Metadata about the log source."""
    pod_name: str
    container_name: Optional[str]
    namespace: str
    deployment_name: Optional[str]

    class Config:
        from_attributes = True


class LogResult(BaseModel):
    """Result containing logs and metadata."""
    logs: str
    description: str
    metadata: LogMetadata


class PodLogRequest(BaseModel):
    """Request for retrieving logs from a specific pod."""
    namespace: str = Field(..., description="Kubernetes namespace")
    pod_name: str = Field(..., description="Name of the pod")
    container_name: Optional[str] = Field(None, description="Container name (optional)")
    tail_lines: Optional[int] = Field(None, description="Number of lines to retrieve from end")
    since_seconds: Optional[int] = Field(None, description="Return logs newer than this many seconds")
    previous: Optional[bool] = Field(False, description="Return previous terminated container logs")
    limit_bytes: Optional[int] = Field(None, description="Maximum bytes of logs to return")


class DeploymentLogRequest(BaseModel):
    """Request for retrieving logs from failed pods in a deployment."""
    namespace: str = Field(..., description="Kubernetes namespace")
    deployment_name: str = Field(..., description="Name of the deployment")
    fallback: Optional[bool] = Field(False, description="Fallback to any pod if no failed pods found")


class LogResponse(BaseModel):
    """Response containing logs and metadata."""
    logs: str
    description: str
    metadata: LogMetadata

    @classmethod
    def from_log_result(cls, result: LogResult) -> "LogResponse":
        """Create LogResponse from LogResult."""
        return cls(
            logs=result.logs,
            description=result.description,
            metadata=result.metadata
        )
