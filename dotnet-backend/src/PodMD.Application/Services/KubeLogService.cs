using System.IO;
using k8s;
using k8s.Models;
using PodMD.Application.Dtos;
using PodMD.Application.Interfaces;
using PodMD.Shared;

namespace PodMD.Application.Services;

public class KubeLogService : IKubeLogService
{
    private readonly IKubeClientFactory _clientFactory;

    public KubeLogService(IKubeClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task<LogResult> GetPodLogsAsync(Guid clusterId, PodLogParameters parameters)
    {
        var client = await _clientFactory.CreateClientAsync(clusterId);

        // Always get pod information for description
        var pod = await client.CoreV1.ReadNamespacedPodAsync(parameters.PodName, parameters.Namespace);

        // Read logs using the correct API
        using var logStream = await client.CoreV1.ReadNamespacedPodLogAsync(
            parameters.PodName,
            parameters.Namespace,
            container: parameters.ContainerName,
            tailLines: parameters.TailLines,
            sinceSeconds: parameters.SinceSeconds,
            previous: parameters.Previous,
            limitBytes: parameters.LimitBytes);

        using var reader = new StreamReader(logStream);
        var logs = await reader.ReadToEndAsync();

        // Get recent events for this pod (last 10 events)
        var events = await client.CoreV1.ListNamespacedEventAsync(parameters.Namespace);
        var podEvents = events.Items
            .Where(e => e.InvolvedObject.Kind == "Pod" && e.InvolvedObject.Name == parameters.PodName)
            .OrderByDescending(e => e.LastTimestamp ?? e.FirstTimestamp)
            .Take(10)
            .ToList();

        // Always create comprehensive description for LLM analysis
        var descriptionBuilder = new System.Text.StringBuilder();
        descriptionBuilder.AppendLine($"Pod: {pod.Metadata.Name}");
        descriptionBuilder.AppendLine($"Namespace: {pod.Metadata.NamespaceProperty}");
        descriptionBuilder.AppendLine($"Node: {pod.Spec.NodeName ?? "Not scheduled"}");
        descriptionBuilder.AppendLine($"Status: {pod.Status.Phase}");
        descriptionBuilder.AppendLine($"Start Time: {pod.Status.StartTime?.ToString("yyyy-MM-dd HH:mm:ss UTC") ?? "Unknown"}");

        if (pod.Metadata.Labels?.Any() == true)
        {
            descriptionBuilder.AppendLine($"Labels: {string.Join(", ", pod.Metadata.Labels.Select(l => $"{l.Key}={l.Value}"))}");
        }

        descriptionBuilder.AppendLine();
        descriptionBuilder.AppendLine("CONTAINERS:");

        foreach (var container in pod.Spec.Containers)
        {
            var status = pod.Status.ContainerStatuses?.FirstOrDefault(s => s.Name == container.Name);

            descriptionBuilder.AppendLine($"Container: {container.Name}");
            descriptionBuilder.AppendLine($"  Image: {container.Image}");

            if (container.Command?.Any() == true)
            {
                descriptionBuilder.AppendLine($"  Command: {string.Join(" ", container.Command)}");
            }

            if (container.Args?.Any() == true)
            {
                descriptionBuilder.AppendLine($"  Args: {string.Join(" ", container.Args)}");
            }

            // Resource limits and requests
            if (container.Resources?.Limits?.Any() == true)
            {
                var limits = string.Join(", ", container.Resources.Limits.Select(r => $"{r.Key}={r.Value}"));
                descriptionBuilder.AppendLine($"  Resource Limits: {limits}");
            }

            if (container.Resources?.Requests?.Any() == true)
            {
                var requests = string.Join(", ", container.Resources.Requests.Select(r => $"{r.Key}={r.Value}"));
                descriptionBuilder.AppendLine($"  Resource Requests: {requests}");
            }

            // Container state and restarts
            if (status != null)
            {
                descriptionBuilder.AppendLine($"  Ready: {status.Ready}");
                descriptionBuilder.AppendLine($"  Restart Count: {status.RestartCount}");

                if (status.State != null)
                {
                    descriptionBuilder.Append($"  State: ");
                    if (status.State.Running != null)
                    {
                        descriptionBuilder.AppendLine($"Running (since {status.State.Running.StartedAt?.ToString("yyyy-MM-dd HH:mm:ss UTC") ?? "unknown"})");
                    }
                    else if (status.State.Waiting != null)
                    {
                        descriptionBuilder.AppendLine($"Waiting ({status.State.Waiting.Reason ?? "Unknown"}: {status.State.Waiting.Message ?? "No message"})");
                    }
                    else if (status.State.Terminated != null)
                    {
                        descriptionBuilder.AppendLine($"Terminated ({status.State.Terminated.Reason ?? "Unknown"}, exit code {status.State.Terminated.ExitCode})");
                        if (status.State.Terminated.StartedAt.HasValue && status.State.Terminated.FinishedAt.HasValue)
                        {
                            var duration = status.State.Terminated.FinishedAt.Value - status.State.Terminated.StartedAt.Value;
                            descriptionBuilder.AppendLine($"  Runtime: {duration.TotalSeconds} seconds");
                        }
                    }
                    else
                    {
                        descriptionBuilder.AppendLine("Unknown");
                    }
                }
            }
            descriptionBuilder.AppendLine();
        }

        if (podEvents.Any())
        {
            descriptionBuilder.AppendLine("RECENT EVENTS:");
            foreach (var evt in podEvents)
            {
                var timestamp = evt.LastTimestamp ?? evt.FirstTimestamp;
                descriptionBuilder.AppendLine($"{evt.Type,-7} {evt.Reason,-12} {timestamp?.ToString("HH:mm:ss") ?? "unknown",-8} {evt.Message}");
            }
            descriptionBuilder.AppendLine();
        }

        var description = descriptionBuilder.ToString();

        return new LogResult(
            logs,
            description,
            new LogMetadata(
                parameters.PodName,
                parameters.ContainerName,
                parameters.Namespace,
                null
            )
        );
    }

    public async Task<LogResult> GetDeploymentLogsAsync(Guid clusterId, DeploymentLogParameters parameters)
    {
        var client = await _clientFactory.CreateClientAsync(clusterId);

        // Always get deployment information for description
        var deployment = await client.AppsV1.ReadNamespacedDeploymentAsync(parameters.DeploymentName, parameters.Namespace);

        // Find pods for this deployment
        var podList = await client.CoreV1.ListNamespacedPodAsync(
            parameters.Namespace,
            labelSelector: $"app={parameters.DeploymentName}"); // Common label selector

        // Find failed pods (not Running or Succeeded)
        var failedPods = podList.Items
            .Where(pod => pod.Status.Phase != "Running" && pod.Status.Phase != "Succeeded")
            .ToList();

        if (!failedPods.Any())
        {
            if (parameters.Fallback != true)
            {
                throw new InvalidOperationException($"No failed pods found for deployment '{parameters.DeploymentName}' in namespace '{parameters.Namespace}'.");
            }

            // Fallback: get logs from the first pod (typically the most recent)
            var firstPod = podList.Items.FirstOrDefault();
            if (firstPod == null)
            {
                throw new InvalidOperationException($"No pods found for deployment '{parameters.DeploymentName}' in namespace '{parameters.Namespace}'.");
            }

            return await GetLogsFromPodAsync(client, firstPod, parameters.Namespace, parameters.DeploymentName, deployment);
        }

        // Get logs from the first failed pod
        var failedPod = failedPods.First();
        return await GetLogsFromPodAsync(client, failedPod, parameters.Namespace, parameters.DeploymentName, deployment);
    }

    private async Task<LogResult> GetLogsFromPodAsync(
        IKubernetes client,
        V1Pod pod,
        string @namespace,
        string deploymentName,
        V1Deployment? deployment)
    {
        // Determine which container to get logs from
        string? containerName = null;
        var containers = pod.Spec.Containers;

        if (containers.Count == 1)
        {
            // Single container - use it
            containerName = containers[0].Name;
        }
        else if (containers.Count > 1)
        {
            // Multiple containers - try to find a failing one
            var containerStatuses = pod.Status.ContainerStatuses ?? new List<V1ContainerStatus>();

            // Look for containers that are not ready or have restart count > 0
            var failingContainers = containerStatuses
                .Where(cs => !cs.Ready || cs.RestartCount > 0)
                .Select(cs => cs.Name)
                .ToList();

            if (failingContainers.Count == 1)
            {
                containerName = failingContainers[0];
            }
            else if (failingContainers.Count > 1)
            {
                throw new InvalidOperationException(
                    $"Multiple failing containers found in pod '{pod.Metadata.Name}'. Please specify container name explicitly.");
            }
            else
            {
                // No clearly failing containers, use the first one
                containerName = containers[0].Name;
            }
        }

        // Read logs from the selected container
        using var logStream = await client.CoreV1.ReadNamespacedPodLogAsync(
            pod.Metadata.Name,
            @namespace,
            container: containerName);

        using var reader = new StreamReader(logStream);
        var logs = await reader.ReadToEndAsync();

        // Always create detailed description including deployment information
        var descriptionBuilder = new System.Text.StringBuilder();

        if (deployment != null)
        {
            descriptionBuilder.AppendLine($"Deployment Name: {deployment.Metadata.Name}");
            descriptionBuilder.AppendLine($"Deployment Namespace: {deployment.Metadata.NamespaceProperty}");
            descriptionBuilder.AppendLine($"Replicas: {deployment.Status.Replicas}/{deployment.Spec.Replicas}");
            descriptionBuilder.AppendLine($"Strategy: {deployment.Spec.Strategy?.Type ?? "RollingUpdate"}");
            descriptionBuilder.AppendLine();
        }

        descriptionBuilder.AppendLine($"Pod Name: {pod.Metadata.Name}");
        descriptionBuilder.AppendLine($"Pod Status: {pod.Status.Phase}");
        descriptionBuilder.AppendLine($"Pod Labels: {string.Join(", ", pod.Metadata.Labels?.Select(l => $"{l.Key}={l.Value}") ?? Array.Empty<string>())}");

        descriptionBuilder.AppendLine("Pod Containers:");
        foreach (var container in pod.Spec.Containers)
        {
            descriptionBuilder.AppendLine($"- {container.Name}: {container.Image}");
            if (container.Ports?.Any() == true)
            {
                descriptionBuilder.AppendLine($"  Ports: {string.Join(", ", container.Ports.Select(p => $"{p.ContainerPort}/{p.Protocol?.ToLower() ?? "tcp"}"))}");
            }
        }

        if (pod.Status.ContainerStatuses != null && pod.Status.ContainerStatuses.Any())
        {
            descriptionBuilder.AppendLine("Container Statuses:");
            foreach (var status in pod.Status.ContainerStatuses)
            {
                descriptionBuilder.AppendLine($"- {status.Name}: Ready={status.Ready}, Restarts={status.RestartCount}");
                if (status.State != null)
                {
                    descriptionBuilder.Append($"  State: ");
                    if (status.State.Running != null) descriptionBuilder.AppendLine($"Running (since {status.State.Running.StartedAt})");
                    else if (status.State.Waiting != null) descriptionBuilder.AppendLine($"Waiting ({status.State.Waiting.Reason}: {status.State.Waiting.Message})");
                    else if (status.State.Terminated != null) descriptionBuilder.AppendLine($"Terminated ({status.State.Terminated.Reason}, exit code {status.State.Terminated.ExitCode})");
                    else descriptionBuilder.AppendLine("Unknown");
                }
            }
        }

        var description = descriptionBuilder.ToString();

        return new LogResult(
            logs,
            description,
            new LogMetadata(
                pod.Metadata.Name,
                containerName,
                @namespace,
                deploymentName
            )
        );
    }
}
