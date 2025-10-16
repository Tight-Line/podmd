"""
PodMD Python Backend - KubeCluster Entity Model
SQLAlchemy KubeCluster model with one-to-one relationship to Source.
"""

from __future__ import annotations

from sqlalchemy import Column, Text, Boolean, String, ForeignKey
from sqlalchemy.dialects.postgresql import UUID
from sqlalchemy.orm import relationship
from app.database import Base


class KubeCluster(Base):
    """
    Kubernetes cluster configuration entity.

    One-to-one relationship with Source - each KubeCluster maps to exactly one Source
    with type="Kubernetes". The KubeCluster ID is the same as the Source ID.
    """

    __tablename__ = "kube_clusters"

    # Primary key is the same as the Source ID (one-to-one relationship)
    id = Column(UUID(as_uuid=True), ForeignKey("sources.id", ondelete="CASCADE"), primary_key=True)

    # Kubernetes-specific fields (encrypted/store securely)
    bearer_token_enc = Column(Text, nullable=False)  # Encrypted bearer token
    certificate_authority_pem = Column(Text, nullable=True)  # CA certificate string
    insecure_skip_tls_verify = Column(Boolean, nullable=False, default=False)
    default_namespace = Column(String(100), nullable=True)

    # One-to-one relationship with Source
    source = relationship("Source", back_populates="kube_cluster", uselist=False)

    def __repr__(self):
        return f"<KubeCluster(id={self.id}, name={self.source.name if self.source else 'N/A'})>"
