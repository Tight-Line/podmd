using System.ComponentModel.DataAnnotations;

namespace PodMD.Api.Dtos;

public record DeploymentAnalysisRequestDto(
    [Required]
    string Namespace,

    [Required]
    string DeploymentName,

    bool? Fallback
);
