"""
SQLAlchemy database models for PodMD.

Defines User, Source, and KubeCluster models with PostgreSQL-specific features.
"""

import uuid
from enum import Enum
from sqlalchemy import Column, String, Boolean, Integer, Text, DateTime, ForeignKey, Index
from sqlalchemy.orm import relationship
from sqlalchemy.dialects.postgresql import UUID

from .database import Base


class SourceType(Enum):
    """Enumeration for source types."""
    KUBERNETES = "Kubernetes"
    JENKINS = "Jenkins"


class User(Base):
    """
    User model representing application users.

    Stores user authentication and profile information.
    """
    __tablename__ = "users"

    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    email = Column(String(255), unique=True, nullable=False, index=True)
    hashed_password = Column(String(255), nullable=False)
    is_active = Column(Boolean, nullable=False, default=True)
    created_at = Column(DateTime(timezone=True), nullable=False)
    updated_at = Column(DateTime(timezone=True), nullable=False)

    # Relationships
    sources = relationship("Source", back_populates="user")

    def __repr__(self):
        return f"<User(id={self.id}, email={self.email})>"


class Source(Base):
    """
    Source model representing external sources (Kubernetes clusters, Jenkins servers).

    Base configuration for external integrations.
    """
    __tablename__ = "sources"

    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    user_id = Column(UUID(as_uuid=True), ForeignKey("users.id", ondelete="CASCADE"), nullable=False, index=True)
    type = Column(String(50), nullable=False)  # Using string instead of Enum for flexibility
    name = Column(String(100), nullable=False, index=True)
    server = Column(String(500), nullable=False)  # URL field
    key_version = Column(Integer, nullable=False, default=1)
    instructions = Column(Text)
    response_format = Column(String(100))
    created_at = Column(DateTime(timezone=True), nullable=False)
    updated_at = Column(DateTime(timezone=True), nullable=False)

    # Relationships
    user = relationship("User", back_populates="sources")
    kube_cluster = relationship("KubeCluster", back_populates="source", uselist=False)

    def __repr__(self):
        return f"<Source(id={self.id}, name={self.name}, type={self.type})>"


class KubeCluster(Base):
    """
    KubeCluster model extending Source for Kubernetes-specific configuration.

    Stores encrypted credentials for Kubernetes cluster access.
    One-to-one relationship with Source.
    """
    __tablename__ = "kube_clusters"

    id = Column(UUID(as_uuid=True), ForeignKey("sources.id", ondelete="CASCADE"), primary_key=True)
    bearer_token_enc = Column(Text, nullable=False)  # Encrypted bearer token
    certificate_authority_pem = Column(Text)  # Certificate authority PEM
    insecure_skip_tls_verify = Column(Boolean, nullable=False, default=False)
    default_namespace = Column(String(100))

    # Relationships
    source = relationship("Source", back_populates="kube_cluster")

    def __repr__(self):
        return f"<KubeCluster(id={self.id})>"


# Create indexes for better query performance
Index('ix_sources_user_id_type', Source.user_id, Source.type)
Index('ix_sources_name', Source.name)
