using System.ComponentModel.DataAnnotations;

namespace PodMD.Domain.Entities;

public class Source
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Type { get; set; } = string.Empty; // "Kubernetes" or "Jenkins"

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Url]
    public string Server { get; set; } = string.Empty;

    [Required]
    public int KeyVersion { get; set; }

    public string? Instructions { get; set; }

    public string? ResponseFormat { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    // Many-to-many navigation property
    public ICollection<KnowledgeBase> KnowledgeBases { get; set; } = new List<KnowledgeBase>();
}
