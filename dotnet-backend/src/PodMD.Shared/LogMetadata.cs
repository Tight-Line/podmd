namespace PodMD.Shared;

public record LogMetadata(
    string PodName,
    string? ContainerName,
    string Namespace,
    string? DeploymentName
);
