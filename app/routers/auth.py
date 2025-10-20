"""
Authentication router with JWT token support.

Provides user registration and login endpoints with proper password hashing
and JWT token generation.
"""

from datetime import datetime, timedelta, timezone
from sqlalchemy.exc import IntegrityError
from sqlalchemy.ext.asyncio import AsyncSession
from fastapi import APIRouter, Depends, HTTPException, status
from fastapi.security import OAuth2PasswordBearer
from jose import jwt, JWTError
from passlib.context import CryptContext
from pydantic import EmailStr

from app.config import settings
from app.database import get_db
from app.models import User


pwd_context = CryptContext(schemes=["argon2"], deprecated="auto")


class AuthRequest:
    """Base authentication request with email and password."""
    email: EmailStr
    password: str


class RegisterRequest(AuthRequest):
    """Request model for user registration."""
    email: EmailStr
    password: str

    class Config:
        from_attributes = True


class LoginRequest(AuthRequest):
    """Request model for user login."""
    email: EmailStr
    password: str


class Token:
    """Response model for authentication tokens."""
    access_token: str
    token_type: str


# OAuth2 scheme for JWT tokens (optional, for token validation)
oauth2_scheme = OAuth2PasswordBearer(tokenUrl="/auth/login")


def hash_password(password: str) -> str:
    """
    Hash a plain text password.

    Args:
        password: The plain text password to hash

    Returns:
        str: The hashed password
    """
    return pwd_context.hash(password)


def verify_password(plain_password: str, hashed_password: str) -> bool:
    """
    Verify a plain password against a hashed password.

    Args:
        plain_password: The plain text password
        hashed_password: The hashed password from database

    Returns:
        bool: True if passwords match, False otherwise
    """
    return pwd_context.verify(plain_password, hashed_password)


def create_access_token(data: dict, expires_delta: timedelta | None = None) -> str:
    """
    Create a JWT access token.

    Args:
        data: Data to encode in JWT
        expires_delta: Optional expiration time delta

    Returns:
        str: The JWT token
    """
    to_encode = data.copy()
    if expires_delta:
        expire = datetime.now(timezone.utc) + expires_delta
    else:
        expire = datetime.now(timezone.utc) + timedelta(minutes=15)
    to_encode.update({"exp": expire, "iat": datetime.now(timezone.utc)})

    encoded_jwt = jwt.encode(to_encode, settings.jwt_secret_key, algorithm="HS256")
    return encoded_jwt


async def authenticate_user(email: EmailStr, password: str, session: AsyncSession) -> User | None:
    """
    Authenticate a user by email and password.

    Args:
        email: User's email address
        password: Plain text password
        session: Database session

    Returns:
        User | None: The user if authenticated, None otherwise
    """
    user = await session.get(User, email=email)
    if not user:
        return None
    if not verify_password(password, user.hashed_password):
        return None
    return user


router = APIRouter(prefix="/auth", tags=["auth"])


@router.post("/register", response_model=dict, status_code=status.HTTP_201_CREATED)
async def register(request: RegisterRequest, session: AsyncSession = Depends(get_db)) -> dict:
    """
    Register a new user account.

    - Validates email format
    - Ensures password meets minimum requirements
    - Creates user in database with hashed password

    Raises:
        HTTPException: 409 if email already exists
        HTTPException: 422 if validation fails
    """
    # Password validation: minimum 8 characters
    if len(request.password) < 8:
        raise HTTPException(
            status_code=status.HTTP_422_UNPROCESSABLE_ENTITY,
            detail="Password must be at least 8 characters long"
        )

    # Hash the password
    hashed_password = hash_password(request.password)

    # Create new user
    user = User(email=request.email, hashed_password=hashed_password)

    try:
        session.add(user)
        await session.commit()
        await session.refresh(user)
    except IntegrityError:
        await session.rollback()
        raise HTTPException(
            status_code=status.HTTP_409_CONFLICT,
            detail="Email already registered"
        )

    return {"message": "User registered successfully"}


@router.post("/login", response_model=Token)
async def login(request: LoginRequest, session: AsyncSession = Depends(get_db)) -> Token:
    """
    Authenticate user and return JWT token.

    Raises:
        HTTPException: 401 if credentials are invalid
    """
    user = await authenticate_user(request.email, request.password, session)
    if not user:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Invalid credentials",
            headers={"WWW-Authenticate": "Bearer"}
        )

    # Create access token valid for configured duration
    access_token_expires = timedelta(minutes=settings.access_token_expire_minutes)
    access_token = create_access_token(
        data={"sub": user.email}, expires_delta=access_token_expires
    )

    return Token(access_token=access_token, token_type="bearer")
