using System.ComponentModel.DataAnnotations;

namespace PodMD.Domain.Entities;

public sealed class ApiKey
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [StringLength(64)] // SHA-256 hash as hex string
    public string HashedKey { get; set; } = string.Empty;

    public string? Permissions { get; set; }

    public int? UsageLimit { get; set; }

    [Range(0, int.MaxValue)]
    public int UsageCount { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "active";

    [Required]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; }

    public DateTimeOffset? LastUsedAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }

    // Navigation property to ApplicationUser
    public ApplicationUser User { get; set; } = null!;
}
