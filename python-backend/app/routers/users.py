"""
PodMD Python Backend - Users Router
User profile management endpoints.
"""

from fastapi import APIRouter, Depends

from app.models.user import User
from app.schemas.user import UserResponse
from app.auth import get_current_user

router = APIRouter(prefix="/users", tags=["users"])


@router.get("/me", response_model=UserResponse)
async def read_users_me(current_user: User = Depends(get_current_user)):
    """Get current user profile."""
    import logging
    logger = logging.getLogger(__name__)
    logger.info(f"GET /users/me called for user: {current_user.email}")
    return current_user
