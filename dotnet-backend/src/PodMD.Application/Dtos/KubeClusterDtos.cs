using System.ComponentModel.DataAnnotations;

namespace PodMD.Application.Dtos;

public record CreateKubeClusterRequest : SourceCreateDto
{
    public CreateKubeClusterRequest(
        string Name,
        string Server,
        string? Instructions,
        string? ResponseFormat,
        string BearerToken,
        string? CertificateAuthorityPem,
        bool InsecureSkipTlsVerify,
        string? DefaultNamespace)
        : base(Name, Server, Instructions, ResponseFormat)
    {
        this.BearerToken = BearerToken;
        this.CertificateAuthorityPem = CertificateAuthorityPem;
        this.InsecureSkipTlsVerify = InsecureSkipTlsVerify;
        this.DefaultNamespace = DefaultNamespace;
    }

    public string BearerToken { get; init; } = string.Empty;

    public string? CertificateAuthorityPem { get; init; }

    public bool InsecureSkipTlsVerify { get; init; }

    public string? DefaultNamespace { get; init; }
}

public record UpdateKubeClusterRequest(
    string? Name,
    string? Server,
    string? Instructions,
    string? ResponseFormat,
    string? BearerToken,
    string? CertificateAuthorityPem,
    bool? InsecureSkipTlsVerify,
    string? DefaultNamespace
);

public record KubeClusterResponse : SourceReadDto
{
    public KubeClusterResponse(
        SourceReadDto source,
        bool HasCertificateAuthority,
        bool InsecureSkipTlsVerify,
        string? DefaultNamespace)
        : base(
            source.Id,
            source.Name,
            source.Type,
            source.Server,
            source.Instructions,
            source.ResponseFormat,
            source.CreatedAt,
            source.UpdatedAt)
    {
        this.HasCertificateAuthority = HasCertificateAuthority;
        this.InsecureSkipTlsVerify = InsecureSkipTlsVerify;
        this.DefaultNamespace = DefaultNamespace;
    }

    public bool HasBearerToken => true; // Always true since bearer token exists but is encrypted
    public bool HasCertificateAuthority { get; init; }
    public bool InsecureSkipTlsVerify { get; init; }
    public string? DefaultNamespace { get; init; }
}
