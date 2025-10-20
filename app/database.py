"""
Database configuration and session management for PodMD.

Provides SQLAlchemy async engine setup, session management, and base model class.
"""

import datetime
from sqlalchemy import event
from sqlalchemy.ext.asyncio import AsyncSession, create_async_engine, async_sessionmaker
from sqlalchemy.orm import DeclarativeBase

from .config import settings


class Base(DeclarativeBase):
    """
    Base class for all database models.

    Provides common metadata and automatic table naming.
    """

    # Subclasses will have __tablename__ inferred from class name
    # e.g. UserSource -> user_sources


@event.listens_for(Base, 'before_insert', propagate=True)
def set_created_at(mapper, connection, target):
    if hasattr(target, 'created_at') and target.created_at is None:
        target.created_at = datetime.datetime.utcnow()


@event.listens_for(Base, 'before_update', propagate=True)
def set_updated_at(mapper, connection, target):
    if hasattr(target, 'updated_at'):
        target.updated_at = datetime.datetime.utcnow()


# Create async engine with configuration from settings
engine = create_async_engine(
    settings.database_url,
    echo=False,  # Set to True for SQL query logging in development
    future=True,
)

# Create async session factory
async_session = async_sessionmaker(
    bind=engine,
    class_=AsyncSession,
    expire_on_commit=False,  # Don't expire objects after commit
)


async def get_db() -> AsyncSession:
    """
    Dependency function for FastAPI to inject database sessions.

    Usage:
        async def endpoint(db: AsyncSession = Depends(get_db)):
            ...

    This provides a database session that automatically closes after the request.
    """
    async with async_session() as session:
        try:
            yield session
        finally:
            await session.close()
