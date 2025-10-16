"""
PodMD Python Backend - KubeLogService
Service for retrieving logs from Kubernetes pods and deployments.
"""

import logging
from typing import Optional, List, Dict, Any
from datetime import datetime, timezone

# Import kubernetes at module level to avoid dependency injection issues
try:
    import kubernetes as k8s
    from kubernetes.client import ApiClient
    from kubernetes.client.models import (
        V1Pod, V1Deployment, V1PodList,
        V1ContainerState, V1ContainerStatus, V1ResourceRequirements
    )
    KUBERNETES_AVAILABLE = True
except ImportError as e:
    logger = logging.getLogger(__name__)
    logger.error(f"Failed to import kubernetes: {e}")
    KUBERNETES_AVAILABLE = False

from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select

from app.schemas.log import LogResult, LogMetadata, PodLogRequest, DeploymentLogRequest
from app.models.kube_cluster import KubeCluster

logger = logging.getLogger(__name__)


class KubeLogService:
    """Service for retrieving Kubernetes logs."""

    @staticmethod
    def create_kubernetes_client(cluster: KubeCluster) -> ApiClient:
        """Create a Kubernetes API client from cluster configuration."""
        logger.info(f"Creating Kubernetes client for cluster with server: {cluster.server}")
        config = k8s.client.Configuration()

        # Set server URL
        config.host = cluster.server

        # Set authentication
        if cluster.bearer_token_enc:
            config.api_key['authorization'] = cluster.bearer_token_enc
            config.api_key_prefix['authorization'] = 'Bearer'

        # Configure SSL/TLS
        if cluster.certificate_authority_pem:
            import base64
            import tempfile
            import os

            # Decode and save CA certificate to temp file
            ca_cert_data = cluster.certificate_authority_pem
            if not ca_cert_data.startswith("-----BEGIN CERTIFICATE-----"):
                # Assume base64 encoded
                ca_cert_data = base64.b64decode(ca_cert_data).decode('utf-8')

            with tempfile.NamedTemporaryFile(mode='w', delete=False, suffix='.pem') as ca_file:
                ca_file.write(ca_cert_data)
                ca_file_path = ca_file.name

            config.ssl_ca_cert = ca_file_path
            config.verify_ssl = True

            # Clean up temp file after client creation
            def cleanup():
                try:
                    os.unlink(ca_file_path)
                except:
                    pass
        else:
            # Skip SSL verification if requested
            config.verify_ssl = not cluster.insecure_skip_tls_verify

        # Create and return client
        logger.info(f"Created Kubernetes client for cluster at {config.host} (SSL verify: {config.verify_ssl})")
        return k8s.client.ApiClient(config)

    async def get_pod_logs_async(self, cluster_id: str, request: PodLogRequest, db: AsyncSession) -> LogResult:
        """Get logs from a specific pod and container."""
        # Get cluster configuration
        cluster = await self._get_cluster(cluster_id, db)
        if not cluster:
            raise ValueError(f"Cluster {cluster_id} not found")

        with self.create_kubernetes_client(cluster) as client:
            core_v1 = k8s.client.CoreV1Api(client)

            # Get pod information
            pod = core_v1.read_namespaced_pod(request.pod_name, request.namespace)

            # Read logs
            log_response = core_v1.read_namespaced_pod_log(
                name=request.pod_name,
                namespace=request.namespace,
                container=request.container_name,
                tail_lines=request.tail_lines,
                since_seconds=request.since_seconds,
                previous=request.previous,
                limit_bytes=request.limit_bytes
            )

            # Get logs from response
            logs = log_response if isinstance(log_response, str) else ""

            # Get recent events
            events = core_v1.list_namespaced_event(request.namespace)
            pod_events = [
                evt for evt in events.items
                if evt.involved_object.kind == "Pod" and evt.involved_object.name == request.pod_name
            ][:10]  # Limit to 10 events

            # Create description
            description = self._build_pod_description(pod, pod_events, request.namespace)

            return LogResult(
                logs=logs,
                description=description,
                metadata=LogMetadata(
                    pod_name=pod.metadata.name,
                    container_name=request.container_name,
                    namespace=request.namespace,
                    deployment_name=None
                )
            )

    async def get_deployment_logs_async(self, cluster_id: str, request: DeploymentLogRequest, db: AsyncSession) -> LogResult:
        """Get logs from failed pods in a deployment."""
        logger.info(f"Starting deployment log retrieval for cluster {cluster_id}, deployment {request.deployment_name}, namespace {request.namespace}")

        # Get cluster configuration
        cluster = await self._get_cluster(cluster_id, db)
        if not cluster:
            logger.error(f"Cluster {cluster_id} not found in database")
            raise ValueError(f"Cluster {cluster_id} not found")

        logger.info(f"Connecting to Kubernetes cluster {cluster.server}")
        with self.create_kubernetes_client(cluster) as client:
            core_v1 = k8s.client.CoreV1Api(client)
            apps_v1 = k8s.client.AppsV1Api(client)

            # Get deployment information
            logger.info(f"Fetching deployment {request.deployment_name} in namespace {request.namespace}")
            deployment = apps_v1.read_namespaced_deployment(
                request.deployment_name, request.namespace)
            logger.info(f"Found deployment: {deployment.metadata.name}, replicas: {deployment.spec.replicas}")

            # Find pods for this deployment
            logger.info(f"Looking for pods with label app={request.deployment_name}")
            pod_list = core_v1.list_namespaced_pod(
                namespace=request.namespace,
                label_selector=f"app={request.deployment_name}"
            )
            logger.info(f"Found {len(pod_list.items)} pods for deployment")

            # Find failed pods (not Running or Succeeded)
            failed_pods = [
                pod for pod in pod_list.items
                if pod.status.phase not in ["Running", "Succeeded"]
            ]
            logger.info(f"Found {len(failed_pods)} failed pods out of {len(pod_list.items)} total pods")

            if not failed_pods:
                if not request.fallback:
                    logger.warning(f"No failed pods found and fallback disabled for deployment '{request.deployment_name}'")
                    raise ValueError(
                        f"No failed pods found for deployment '{request.deployment_name}' "
                        f"in namespace '{request.namespace}'."
                    )
                # Fallback: use first pod
                selected_pod = pod_list.items[0] if pod_list.items else None
                logger.info(f"Using fallback pod: {selected_pod.metadata.name if selected_pod else 'None'}")
            else:
                selected_pod = failed_pods[0]
                logger.info(f"Selected failed pod: {selected_pod.metadata.name} (status: {selected_pod.status.phase})")

            if not selected_pod:
                logger.error(f"No pods available for deployment '{request.deployment_name}'")
                raise ValueError(
                    f"No pods found for deployment '{request.deployment_name}' "
                    f"in namespace '{request.namespace}'."
                )

            # Get logs from selected pod
            logger.info(f"Retrieving logs from pod {selected_pod.metadata.name}")
            return await self._get_logs_from_pod(
                client, selected_pod, request.namespace, request.deployment_name, deployment)

    async def _get_logs_from_pod(
        self,
        client: ApiClient,
        pod: V1Pod,
        namespace: str,
        deployment_name: str,
        deployment: Optional[V1Deployment] = None
    ) -> LogResult:
        """Get logs from a specific pod with deployment context."""
        core_v1 = k8s.client.CoreV1Api(client)

        # Determine container to log from
        container_name = self._select_container(pod)

        # Read logs
        log_response = core_v1.read_namespaced_pod_log(
            name=pod.metadata.name,
            namespace=namespace,
            container=container_name
        )

        logs = log_response if isinstance(log_response, str) else ""

        # Get recent events
        events = core_v1.list_namespaced_event(namespace)
        pod_events = [
            evt for evt in events.items
            if evt.involved_object.kind == "Pod" and evt.involved_object.name == pod.metadata.name
        ][:10]  # Limit to 10 events

        # Build description with deployment context
        description = self._build_deployment_pod_description(
            pod, deployment, pod_events, namespace)

        return LogResult(
            logs=logs,
            description=description,
            metadata=LogMetadata(
                pod_name=pod.metadata.name,
                container_name=container_name,
                namespace=namespace,
                deployment_name=deployment_name
            )
        )

    def _select_container(self, pod: V1Pod) -> Optional[str]:
        """Select which container to get logs from."""
        containers = pod.spec.containers or []

        if len(containers) == 1:
            return containers[0].name

        if len(containers) > 1:
            # Look for failing containers
            container_statuses = pod.status.container_statuses or []
            failing_containers = [
                status for status in container_statuses
                if not status.ready or status.restart_count > 0
            ]

            if len(failing_containers) == 1:
                return failing_containers[0].name

            # Multiple failing containers, pick first one
            if failing_containers:
                return failing_containers[0].name

            # No clearly failing containers, use first one
            return containers[0].name

        return None

    def _build_pod_description(self, pod: V1Pod, events: List, namespace: str) -> str:
        """Build detailed description for pod logs."""
        description = []
        description.append(f"Pod: {pod.metadata.name}")
        description.append(f"Namespace: {namespace}")
        description.append(f"Node: {pod.spec.node_name or 'Not scheduled'}")
        description.append(f"Status: {pod.status.phase}")

        if pod.metadata.labels:
            labels = ", ".join(f"{k}={v}" for k, v in pod.metadata.labels.items())
            description.append(f"Labels: {labels}")

        if pod.status.start_time:
            start_time = pod.status.start_time.replace(tzinfo=timezone.utc)
            description.append(f"Start Time: {start_time.strftime('%Y-%m-%d %H:%M:%S UTC')}")

        description.append("")
        description.append("CONTAINERS:")

        for container in pod.spec.containers:
            description.append(f"Container: {container.name}")
            description.append(f"  Image: {container.image}")

            if container.command:
                description.append(f"  Command: {' '.join(container.command)}")
            if container.args:
                description.append(f"  Args: {' '.join(container.args)}")

            # Resource limits and requests
            if container.resources:
                if container.resources.limits:
                    limits = ", ".join(f"{k}={v}" for k, v in container.resources.limits.items())
                    description.append(f"  Resource Limits: {limits}")
                if container.resources.requests:
                    requests = ", ".join(f"{k}={v}" for k, v in container.resources.requests.items())
                    description.append(f"  Resource Requests: {requests}")

            # Container state
            status = next(
                (s for s in (pod.status.container_statuses or [])
                 if s.name == container.name), None)
            if status:
                description.append(f"  Ready: {status.ready}")
                description.append(f"  Restart Count: {status.restart_count}")

                if status.state:
                    description.append("  State: " + self._format_container_state(status.state))
            description.append("")

        # Recent events
        if events:
            description.append("RECENT EVENTS:")
            for evt in events:
                timestamp = evt.last_timestamp or evt.first_timestamp
                time_str = timestamp.replace(tzinfo=timezone.utc).strftime('%H:%M:%S') if timestamp else "unknown"
                description.append(f"{evt.type or 'Normal':<7} {evt.reason or 'Unknown':<12} {time_str:<8} {evt.message or 'No message'}")
            description.append("")

        return "\n".join(description)

    def _build_deployment_pod_description(
        self, pod: V1Pod, deployment: Optional[V1Deployment], events: List, namespace: str
    ) -> str:
        """Build description for deployment logs with pod information."""
        description = []

        if deployment:
            description.append(f"Deployment Name: {deployment.metadata.name}")
            description.append(f"Deployment Namespace: {namespace}")
            description.append(f"Replicas: {deployment.status.available_replicas or 0}/{deployment.spec.replicas}")
            description.append(f"Strategy: {deployment.spec.strategy.type if deployment.spec.strategy else 'RollingUpdate'}")
            description.append("")

        description.append(f"Pod Name: {pod.metadata.name}")
        description.append(f"Pod Status: {pod.status.phase}")

        if pod.metadata.labels:
            labels = ", ".join(f"{k}={v}" for k, v in pod.metadata.labels.items())
            description.append(f"Pod Labels: {labels}")

        # Container information
        if pod.spec.containers:
            description.append("Pod Containers:")
            for container in pod.spec.containers:
                description.append(f"- {container.name}: {container.image}")
                if container.ports:
                    ports = ", ".join(f"{p.container_port}/{p.protocol or 'tcp'}" for p in container.ports)
                    description.append(f"  Ports: {ports}")
            description.append("")

        # Container statuses
        if pod.status.container_statuses:
            description.append("Container Statuses:")
            for status in pod.status.container_statuses:
                description.append(f"- {status.name}: Ready={status.ready}, Restarts={status.restart_count}")
                if status.state:
                    description.append("  State: " + self._format_container_state(status.state))
            description.append("")

        # Recent events
        if events:
            description.append("RECENT EVENTS:")
            for evt in events:
                timestamp = evt.last_timestamp or evt.first_timestamp
                time_str = timestamp.replace(tzinfo=timezone.utc).strftime('%H:%M:%S') if timestamp else "unknown"
                description.append(f"{evt.type or 'Normal':<7} {evt.reason or 'Unknown':<12} {time_str:<8} {evt.message or 'No message'}")
            description.append("")

        return "\n".join(description)

    def _format_container_state(self, state: V1ContainerState) -> str:
        """Format container state for display."""
        if state.running:
            started = state.running.started_at.replace(tzinfo=timezone.utc) if state.running.started_at else None
            start_str = started.strftime('%Y-%m-%d %H:%M:%S UTC') if started else "unknown"
            return f"Running (since {start_str})"
        elif state.waiting:
            return f"Waiting ({state.waiting.reason or 'Unknown'}: {state.waiting.message or 'No message'})"
        elif state.terminated:
            reason = state.terminated.reason or "Unknown"
            exit_code = state.terminated.exit_code or 0
            return f"Terminated ({reason}, exit code {exit_code})"
        else:
            return "Unknown"

    async def _get_cluster(self, cluster_id: str, db: AsyncSession) -> Optional[KubeCluster]:
        """Get cluster configuration by ID with source join."""
        from sqlalchemy.orm import selectinload
        query = select(KubeCluster).options(selectinload(KubeCluster.source)).where(KubeCluster.id == cluster_id)
        result = await db.execute(query)
        cluster = result.scalar_one_or_none()
        if cluster and cluster.source:
            # Set server from source for convenience
            cluster.server = cluster.source.server
        return cluster
