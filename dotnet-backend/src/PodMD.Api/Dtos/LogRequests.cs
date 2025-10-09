using System.ComponentModel.DataAnnotations;
using PodMD.Shared;

namespace PodMD.Api.Dtos;

public record PodLogRequest(
    [Required]
    string Namespace,

    [Required]
    string PodName,

    string? ContainerName,

    int? TailLines,

    int? SinceSeconds,

    bool? Previous,

    int? LimitBytes
);

public record DeploymentLogRequest(
    [Required]
    string Namespace,

    [Required]
    string DeploymentName,

    bool? Fallback
);

public record LogResponse(
    string Logs,
    string? Description,
    LogMetadata? Metadata
);
