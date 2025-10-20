"""
Kubernetes clusters service layer.

Provides business logic for CRUD operations on KubeCluster entities with
encryption, user isolation, and data validation.
"""

from datetime import datetime, timezone
from typing import List, Optional
from uuid import UUID

from sqlalchemy import select
from sqlalchemy.ext.asyncio import AsyncSession

from app.models import KubeCluster, Source
from app.schemas.kube_clusters import (
    CreateKubeClusterRequest,
    KubeClusterResponse,
    UpdateKubeClusterRequest,
)
from app.utils.crypto import decrypt_token, encrypt_token


class KubeClusterService:
    """
    Service for managing Kubernetes clusters with encryption and user isolation.
    """

    @staticmethod
    async def _encode_response(source: Source, kube_cluster: KubeCluster) -> KubeClusterResponse:
        """
        Encode database entities into API response schema.

        Decrypts the bearer token for display (though UI might not show it).
        """
        return KubeClusterResponse(
            id=source.id,
            name=source.name,
            server=source.server,
            bearer_token_enc=kube_cluster.bearer_token_enc,
            certificate_authority_pem=kube_cluster.certificate_authority_pem,
            insecure_skip_tls_verify=kube_cluster.insecure_skip_tls_verify,
            default_namespace=kube_cluster.default_namespace,
            user_id=source.user_id,
            created_at=source.created_at,
            updated_at=source.updated_at,
        )

    async def create_cluster(
        self, session: AsyncSession, user_id: UUID, cluster_data: CreateKubeClusterRequest
    ) -> KubeClusterResponse:
        """
        Create a new Kubernetes cluster for the user.

        Encrypts the bearer token and stores both Source and KubeCluster records.
        """
        # Encrypt the bearer token
        encrypted_token = encrypt_token(cluster_data.bearer_token)

        # Current timestamp
        current_time = datetime.now(timezone.utc)

        # Create Source record
        source = Source(
            user_id=user_id,
            type="Kubernetes",
            name=cluster_data.name,
            server=str(cluster_data.server),  # Convert HttpUrl to string
            created_at=current_time,
            updated_at=current_time,
        )
        session.add(source)
        await session.flush()  # Get the ID

        # Create KubeCluster record
        kube_cluster = KubeCluster(
            id=source.id,  # Same ID as Source for 1-1 relationship
            bearer_token_enc=encrypted_token,
            certificate_authority_pem=cluster_data.certificate_authority_pem,
            insecure_skip_tls_verify=cluster_data.insecure_skip_tls_verify,
            default_namespace=cluster_data.default_namespace,
        )
        session.add(kube_cluster)

        await session.commit()
        await session.refresh(source)
        await session.refresh(kube_cluster)

        return await self._encode_response(source, kube_cluster)

    async def get_clusters(self, session: AsyncSession, user_id: UUID) -> List[KubeClusterResponse]:
        """
        Retrieve all Kubernetes clusters for the user.
        """
        stmt = select(Source, KubeCluster).where(
            Source.user_id == user_id,
            Source.type == "Kubernetes",
            Source.id == KubeCluster.id,
        )

        results = await session.execute(stmt)
        clusters = []

        for source, kube_cluster in results.tuples():
            clusters.append(await self._encode_response(source, kube_cluster))

        return clusters

    async def get_cluster(
        self, session: AsyncSession, user_id: UUID, cluster_id: UUID
    ) -> Optional[KubeClusterResponse]:
        """
        Retrieve a specific Kubernetes cluster by ID for the user.
        """
        stmt = select(Source, KubeCluster).where(
            Source.id == cluster_id,
            Source.user_id == user_id,
            Source.type == "Kubernetes",
            Source.id == KubeCluster.id,
        )

        result = await session.execute(stmt)
        row = result.first()

        if row:
            source, kube_cluster = row
            return await self._encode_response(source, kube_cluster)

        return None

    async def update_cluster(
        self,
        session: AsyncSession,
        user_id: UUID,
        cluster_id: UUID,
        update_data: UpdateKubeClusterRequest,
    ) -> Optional[KubeClusterResponse]:
        """
        Update a Kubernetes cluster for the user.

        Only updates provided fields, handles bearer token re-encryption if provided.
        """
        # First get the existing cluster
        existing_stmt = select(Source, KubeCluster).where(
            Source.id == cluster_id,
            Source.user_id == user_id,
            Source.type == "Kubernetes",
            Source.id == KubeCluster.id,
        )

        result = await session.execute(existing_stmt)
        row = result.first()

        if not row:
            return None

        source, kube_cluster = row

        # Update Source fields
        if update_data.name is not None:
            source.name = update_data.name
        if update_data.server is not None:
            source.server = str(update_data.server)

        # Update KubeCluster fields
        if update_data.bearer_token is not None:
            kube_cluster.bearer_token_enc = encrypt_token(update_data.bearer_token)
        if update_data.certificate_authority_pem is not None:
            kube_cluster.certificate_authority_pem = update_data.certificate_authority_pem
        if update_data.insecure_skip_tls_verify is not None:
            kube_cluster.insecure_skip_tls_verify = update_data.insecure_skip_tls_verify
        if update_data.default_namespace is not None:
            kube_cluster.default_namespace = update_data.default_namespace

        await session.commit()
        await session.refresh(source)

        return await self._encode_response(source, kube_cluster)

    async def delete_cluster(self, session: AsyncSession, user_id: UUID, cluster_id: UUID) -> bool:
        """
        Delete a Kubernetes cluster for the user.

        Returns True if deleted, False if not found.
        """
        stmt = select(Source, KubeCluster).where(
            Source.id == cluster_id,
            Source.user_id == user_id,
            Source.type == "Kubernetes",
            Source.id == KubeCluster.id,
        )

        result = await session.execute(stmt)
        row = result.first()

        if not row:
            return False

        source, kube_cluster = row

        await session.delete(kube_cluster)
        await session.delete(source)
        await session.commit()

        return True


# Global service instance
cluster_service = KubeClusterService()
