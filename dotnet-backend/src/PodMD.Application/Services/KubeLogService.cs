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

        // Get pod information if description is requested
        V1Pod? pod = null;
        if (parameters.IncludeDescription == true)
        {
            pod = await client.CoreV1.ReadNamespacedPodAsync(parameters.PodName, parameters.Namespace);
        }

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

        // Create description if requested
        string? description = null;
        if (parameters.IncludeDescription == true && pod != null)
        {
            description = $"Pod: {pod.Metadata.Name}, Status: {pod.Status.Phase}, " +
                         $"Containers: {string.Join(", ", pod.Spec.Containers.Select(c => c.Name))}";
        }

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

        // Get deployment information if description is requested
        V1Deployment? deployment = null;
        if (parameters.IncludeDescription == true)
        {
            deployment = await client.AppsV1.ReadNamespacedDeploymentAsync(parameters.DeploymentName, parameters.Namespace);
        }

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

            return await GetLogsFromPodAsync(client, firstPod, parameters.Namespace, parameters.DeploymentName,
                                           parameters.IncludeDescription == true ? deployment : null);
        }

        // Get logs from the first failed pod
        var failedPod = failedPods.First();
        return await GetLogsFromPodAsync(client, failedPod, parameters.Namespace, parameters.DeploymentName,
                                       parameters.IncludeDescription == true ? deployment : null);
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

        // Create description if deployment info is available
        string? description = null;
        if (deployment != null)
        {
            description = $"Deployment: {deployment.Metadata.Name}, " +
                         $"Pod: {pod.Metadata.Name}, Status: {pod.Status.Phase}, " +
                         $"Container: {containerName}";
        }

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
