using PodMD.Shared;

namespace PodMD.Application.Dtos;

public record PodLogParameters(
    string Namespace,
    string PodName,
    string? ContainerName,
    int? TailLines,
    int? SinceSeconds,
    bool? Previous,
    int? LimitBytes
);

public record DeploymentLogParameters(
    string Namespace,
    string DeploymentName,
    bool? Fallback
);

public record LogResult(
    string Logs,
    string? Description,
    LogMetadata? Metadata
);
