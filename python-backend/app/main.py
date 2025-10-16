"""
PodMD Python Backend - FastAPI Implementation
Experimental alternative to the .NET backend using Python/FastAPI with PostgreSQL.
"""

from datetime import datetime
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
import os
import asyncio
import logging
from app.database import engine, Base
from app.routers.auth import router as auth_router
from app.routers.users import router as users_router
from app.routers.kube_cluster import router as kube_cluster_router
from app.routers.logs import router as logs_router, analysis_router
from dotenv import load_dotenv

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

load_dotenv()

async def create_tables():
    """Create all database tables."""
    async with engine.begin() as conn:
        await conn.run_sync(Base.metadata.create_all)
    print("Database tables created successfully!")

app = FastAPI(
    title="PodMD Python Backend",
    description="Experimental Python implementation using FastAPI and PostgreSQL with user authentication",
    version="0.1.0"
)

# CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://localhost:3000", "http://localhost:5173", "http://localhost:5174"],  # Add your frontend URLs
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Include routers
app.include_router(auth_router)
app.include_router(users_router)
app.include_router(kube_cluster_router)
app.include_router(logs_router)
app.include_router(analysis_router)


@app.get("/health")
async def health_check():
    """
    Health check endpoint returning system status.

    Returns:
        dict: Health status with timestamp and service info
    """
    return {
        "status": "healthy",
        "service": "podmd-python-backend",
        "timestamp": datetime.utcnow().isoformat(),
        "version": "0.1.0",
        "database": "configured"  # Database is now configured
    }


@app.get("/")
async def root():
    """
    Root endpoint providing basic API information.

    Returns:
        dict: Welcome message and available endpoints
    """
    return {
        "message": "Welcome to PodMD Python Backend",
        "service": "podmd-python-backend",
        "version": "0.1.0",
        "endpoints": {
            "health": "/health",
            "register": "/auth/register",
            "login": "/auth/login",
            "profile": "/users/me",
            "kube-clusters": "/kube-clusters/",
            "pod-logs": "/kube-clusters/{cluster_id}/logs/pods",
            "deployment-logs": "/kube-clusters/{cluster_id}/logs/deployments",
            "pod-analysis": "/kube-clusters/{cluster_id}/analyze/pods",
            "deployment-analysis": "/kube-clusters/{cluster_id}/analyze/deployments",
            "documentation": "/docs"
        }
    }


@app.on_event("startup")
async def startup_event():
    """Initialize database on startup."""
    async with engine.begin() as conn:
        await conn.run_sync(Base.metadata.create_all)
    logger.info("Database tables created successfully!")
