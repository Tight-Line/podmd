using System.ComponentModel.DataAnnotations;

namespace PodMD.Domain.Entities;

public class KubeCluster
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Url]
    public string Server { get; set; } = string.Empty;

    [Required]
    public string BearerTokenEnc { get; set; } = string.Empty;

    public string? CertificateAuthorityPem { get; set; }

    [Required]
    public bool InsecureSkipTlsVerify { get; set; }

    public string? DefaultNamespace { get; set; }

    public string? Instructions { get; set; }

    public string? ResponseFormat { get; set; }

    [Required]
    public int KeyVersion { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }
}
