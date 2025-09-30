using System.ComponentModel.DataAnnotations;

namespace PodMD.Api.Dtos;

public record PodAnalysisRequestDto(
    [Required]
    string Namespace,

    [Required]
    string PodName,

    string? ContainerName,

    int? TailLines,

    int? SinceSeconds,

    bool? Previous,

    int? LimitBytes,

    bool? IncludeDescription
);
