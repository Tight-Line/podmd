using System.ComponentModel.DataAnnotations;

namespace PodMD.Domain.Entities;

public class JenkinsServers : Source
{
    [Required]
    [StringLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string ApiTokenEnc { get; set; } = string.Empty; // Encrypted API token
}
