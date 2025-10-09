using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PodMD.Domain.Entities;

public class KnowledgeFile
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid KnowledgeBaseId { get; set; }

    [Required]
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string StorageKey { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string ContentType { get; set; } = string.Empty;

    [Required]
    public long FileSize { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    // Navigation property
    [ForeignKey(nameof(KnowledgeBaseId))]
    public KnowledgeBase KnowledgeBase { get; set; } = null!;
}
