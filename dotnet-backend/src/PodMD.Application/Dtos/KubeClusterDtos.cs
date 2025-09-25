using System.ComponentModel.DataAnnotations;

namespace PodMD.Application.Dtos;

public record CreateKubeClusterRequest(
    [Required]
    [StringLength(100)]
    string Name,

    [Required]
    [Url]
    string Server,

    [Required]
    string BearerToken,

    string? CertificateAuthorityPem,

    [Required]
    bool InsecureSkipTlsVerify,

    string? DefaultNamespace
);

public record UpdateKubeClusterRequest(
    string? Name,
    string? Server,
    string? BearerToken,
    string? CertificateAuthorityPem,
    bool? InsecureSkipTlsVerify,
    string? DefaultNamespace
);

public record KubeClusterResponse(
    Guid Id,
    string Name,
    string Server,
    bool HasBearerToken,
    bool HasCertificateAuthority,
    bool InsecureSkipTlsVerify,
    string? DefaultNamespace,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
