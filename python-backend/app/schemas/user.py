"""
PodMD Python Backend - User Pydantic Schemas
Request/response schemas for user operations.
"""

from pydantic import BaseModel, EmailStr
from uuid import UUID
from datetime import datetime
from typing import Optional


class UserBase(BaseModel):
    """Base user schema with email."""
    email: EmailStr


class UserCreate(UserBase):
    """Schema for user registration."""
    password: str


class UserLogin(BaseModel):
    """Schema for user login."""
    email: EmailStr
    password: str


class UserResponse(UserBase):
    """Schema for user response data."""
    id: UUID
    is_active: bool
    created_at: datetime
    updated_at: Optional[datetime] = None

    class Config:
        from_attributes = True


class TokenData(BaseModel):
    """Schema for JWT token payload."""
    sub: Optional[str] = None


class Token(BaseModel):
    """Schema for JWT token response."""
    access_token: str
    token_type: str = "bearer"
