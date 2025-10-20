"""
Main FastAPI application module for PodMD.

Provides the core FastAPI application with middleware, routing, and lifecycle management.
"""

from contextlib import asynccontextmanager
from fastapi import FastAPI, Depends
from fastapi.middleware.cors import CORSMiddleware
from sqlalchemy.ext.asyncio import AsyncSession

from .config import settings
from .database import get_db


@asynccontextmanager
async def lifespan(app: FastAPI):
    """
    Application lifespan context manager.

    Handles startup and shutdown events for the FastAPI application.
    """
    # Startup: Database connection will be tested by health endpoint
    print(f"Starting PodMD on {settings.app_host}:{settings.app_port}")

    yield

    # Shutdown: Clean up resources
    print("Shutting down PodMD")


# Create FastAPI application
app = FastAPI(
    title="PodMD",
    description="Web API service for automated troubleshooting of failed Kubernetes pods/deployments and CI builds",
    version="0.1.0",
    lifespan=lifespan,
)

# Add CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # Configure properly for production
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Import auth router
from app.routers.auth import router as auth_router

# Include auth router
app.include_router(auth_router)

# Health check endpoint - required by Issue #1
@app.get("/health")
async def health_check(session: AsyncSession = Depends(get_db)):
    """
    Health check endpoint that verifies database connectivity.

    Returns:
        dict: Health status with database connection state and timestamp
    """
    from datetime import datetime
    from sqlalchemy import text

    db_status = "connected"

    try:
        # Test database connection
        await session.execute(text("SELECT 1"))
    except Exception as e:
        print(f"DB connection error: {e}")
        db_status = "disconnected"

    return {
        "status": "healthy",
        "database": db_status,
        "timestamp": datetime.utcnow().isoformat() + "Z"
    }


# Root endpoint with API information
@app.get("/")
async def root():
    """
    Root endpoint providing API information.

    Returns basic API metadata and available endpoints.
    """
    return {
        "name": "PodMD",
        "version": "0.1.0",
        "description": "Web API service for automated troubleshooting of failed Kubernetes pods/deployments and CI builds",
        "docs": "/docs",
        "endpoints": {
            "health": "/health"
        }
    }


if __name__ == "__main__":
    # For development: run with uvicorn directly
    import uvicorn

    uvicorn.run(
        "app.main:app",
        host=settings.app_host,
        port=settings.app_port,
        reload=True,
        log_level="info"
    )
