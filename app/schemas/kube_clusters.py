"""
Pydantic schemas for Kubernetes clusters CRUD operations.

Defines request/response models for creating, reading, updating, and deleting
Kubernetes cluster configurations with proper validation.
"""

from datetime import datetime
from typing import List, Optional
from uuid import UUID

from pydantic import BaseModel, Field, HttpUrl


class CreateKubeClusterRequest(BaseModel):
    """
    Schema for creating a new Kubernetes cluster.

    Validates input for cluster creation with required fields and constraints.
    """
    name: str = Field(..., min_length=1, max_length=100)
    server: HttpUrl  # Validates URL format
    bearer_token: str = Field(..., min_length=1)
    certificate_authority_pem: Optional[str] = None
    insecure_skip_tls_verify: bool = False
    default_namespace: Optional[str] = None


class UpdateKubeClusterRequest(BaseModel):
    """
    Schema for updating an existing Kubernetes cluster.

    All fields are optional for flexible partial updates.
    """
    name: Optional[str] = Field(None, min_length=1, max_length=100)
    server: Optional[HttpUrl] = None
    bearer_token: Optional[str] = Field(None, min_length=1)
    certificate_authority_pem: Optional[str] = None
    insecure_skip_tls_verify: Optional[bool] = None
    default_namespace: Optional[str] = None


class KubeClusterResponse(BaseModel):
    """
    Schema for Kubernetes cluster responses.

    Includes all cluster fields in API responses, with encrypted bearer token.
    """
    id: UUID
    name: str
    server: str  # Return as string since HttpUrl serialization is complex
    bearer_token_enc: str
    certificate_authority_pem: Optional[str]
    insecure_skip_tls_verify: bool
    default_namespace: Optional[str]
    user_id: UUID
    created_at: datetime
    updated_at: Optional[datetime]


class KubeClusterListResponse(BaseModel):
    """
    Schema for listing multiple Kubernetes clusters.

    Contains array of cluster responses.
    """
    kube_clusters: List[KubeClusterResponse]
