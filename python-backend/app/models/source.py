"""
PodMD Python Backend - Source Entity Model
SQLAlchemy Source model with user ownership for sources (Kubernetes/Jenkins servers).
"""

from __future__ import annotations

from sqlalchemy import Column, String, DateTime, Integer, Text, ForeignKey
from sqlalchemy.dialects.postgresql import UUID
from sqlalchemy.sql import func
from sqlalchemy.orm import relationship
import uuid
from app.database import Base


class Source(Base):
    """Source entity representing Kubernetes clusters or Jenkins servers with user ownership."""

    __tablename__ = "sources"

    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    user_id = Column(UUID(as_uuid=True), ForeignKey("users.id", ondelete="CASCADE"), nullable=False, index=True)
    type = Column(String(50), nullable=False)  # "Kubernetes" or "Jenkins"
    name = Column(String(100), nullable=False, index=True)
    server = Column(String(500), nullable=False)  # URL field
    key_version = Column(Integer, nullable=False, default=1)
    instructions = Column(Text, nullable=True)
    response_format = Column(String(100), nullable=True)
    created_at = Column(DateTime(timezone=True), server_default=func.now())
    updated_at = Column(DateTime(timezone=True), onupdate=func.now())

    # Relationship to User (owner)
    user = relationship("User")

    # One-to-one relationship with KubeCluster (for Kubernetes sources)
    kube_cluster = relationship("KubeCluster", back_populates="source", uselist=False, cascade="all, delete-orphan")

    def __repr__(self):
        return f"<Source(id={self.id}, name={self.name}, type={self.type})>"
