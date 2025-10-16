"""
PodMD Python Backend - Source Pydantic Schemas
Request/response schemas for source operations (Kubernetes/Jenkins servers).
"""

from pydantic import BaseModel, HttpUrl
from uuid import UUID
from datetime import datetime
from typing import Optional


class SourceBase(BaseModel):
    """Base source schema with common fields."""
    type: str
    name: str
    server: HttpUrl
    key_version: int = 1
    instructions: Optional[str] = None
    response_format: Optional[str] = None


class SourceCreate(BaseModel):
    """Schema for creating a new source."""
    type: str
    name: str
    server: str  # URL as string to parse later
    key_version: int = 1
    instructions: Optional[str] = None
    response_format: Optional[str] = None


class SourceUpdate(BaseModel):
    """Schema for updating an existing source."""
    name: Optional[str] = None
    server: Optional[str] = None  # Will be validated as URL
    key_version: Optional[int] = None
    instructions: Optional[str] = None
    response_format: Optional[str] = None


class SourceResponse(SourceBase):
    """Schema for source response data."""
    id: UUID
    user_id: UUID
    created_at: datetime
    updated_at: Optional[datetime] = None

    class Config:
        from_attributes = True


class SourceListResponse(BaseModel):
    """Schema for source list responses."""
    sources: list[SourceResponse]

    class Config:
        from_attributes = True
