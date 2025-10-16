"""
PodMD Python Backend - KubeCluster Pydantic Schemas
Request/response schemas for KubeCluster operations.
"""

from pydantic import BaseModel, Field
from uuid import UUID
from datetime import datetime
from typing import Optional, Literal
from .source import SourceResponse


class KubeClusterBase(BaseModel):
    """Base Kubernetes cluster schema with core fields."""
    bearer_token: str = Field(..., description="Bearer token for Kubernetes cluster")
    certificate_authority_pem: Optional[str] = Field(None, description="CA certificate PEM string")
    insecure_skip_tls_verify: bool = Field(False, description="Skip TLS certificate verification")
    default_namespace: Optional[str] = Field(None, description="Default namespace for operations")


class KubeClusterCreate(BaseModel):
    """Schema for creating a new Kubernetes cluster with base source fields."""
    name: str = Field(..., min_length=1, max_length=100)
    server: str = Field(..., description="Kubernetes API server URL")
    bearer_token: str = Field(..., description="Bearer token for cluster authentication")
    certificate_authority_pem: Optional[str] = Field(None, description="CA certificate PEM")
    insecure_skip_tls_verify: bool = Field(False, description="Skip TLS verification")
    default_namespace: Optional[str] = Field(None, description="Default Kubernetes namespace")
    instructions: Optional[str] = Field(None, description="Instructions for AI processing")
    response_format: Optional[str] = Field(None, description="Response format for AI")


class KubeClusterUpdate(BaseModel):
    """Schema for updating an existing Kubernetes cluster."""
    name: Optional[str] = Field(None, min_length=1, max_length=100)
    server: Optional[str] = None
    key_version: Optional[int] = Field(None, ge=1)
    instructions: Optional[str] = None
    response_format: Optional[str] = None
    bearer_token: Optional[str] = None  # Plain text bearer token
    certificate_authority_pem: Optional[str] = None
    insecure_skip_tls_verify: Optional[bool] = None
    default_namespace: Optional[str] = None


class KubeClusterResponse(BaseModel):
    """Schema for Kubernetes cluster response data - combined Source and KubeCluster fields."""
    id: UUID
    type: str
    name: str
    server: str
    key_version: int
    instructions: Optional[str] = None
    response_format: Optional[str] = None
    user_id: UUID
    created_at: datetime
    updated_at: Optional[datetime] = None
    # KubeCluster fields
    bearer_token_enc: str
    certificate_authority_pem: Optional[str] = None
    insecure_skip_tls_verify: bool = False
    default_namespace: Optional[str] = None

    class Config:
        from_attributes = True


class KubeClusterListResponse(BaseModel):
    """Schema for Kubernetes cluster list responses."""
    kube_clusters: list[KubeClusterResponse]

    class Config:
        from_attributes = True
