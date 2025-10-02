using System.ComponentModel.DataAnnotations;

namespace PodMD.Domain.Entities;

public class KubeCluster : Source
{
    [Required]
    public string BearerTokenEnc { get; set; } = string.Empty;

    public string? CertificateAuthorityPem { get; set; }

    [Required]
    public bool InsecureSkipTlsVerify { get; set; }

    public string? DefaultNamespace { get; set; }
}
