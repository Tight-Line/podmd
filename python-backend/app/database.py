"""
PodMD Python Backend - Database Configuration
SQLAlchemy engine setup and session management (sync for compatibility).
"""

from sqlalchemy.ext.asyncio import AsyncSession, create_async_engine
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy.orm import sessionmaker
import os
from dotenv import load_dotenv

load_dotenv()

# Use async database URL
DATABASE_URL = os.getenv("DATABASE_URL", "postgresql+asyncpg://podmd_user:password@localhost/podmd_db")

# Create async engine
engine = create_async_engine(DATABASE_URL, echo=False)

# Create async session factory
async_session = sessionmaker(
    bind=engine,
    class_=AsyncSession,
    expire_on_commit=False
)

# Base class for models
Base = declarative_base()


# Dependency to get database session
async def get_db() -> AsyncSession:
    """Dependency for FastAPI dependency injection."""
    async with async_session() as session:
        try:
            yield session
        finally:
            await session.close()
