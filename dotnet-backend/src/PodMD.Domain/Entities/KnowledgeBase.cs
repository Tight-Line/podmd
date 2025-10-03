using System.ComponentModel.DataAnnotations;

namespace PodMD.Domain.Entities;

public class KnowledgeBase
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    // Many-to-many navigation property
    public ICollection<Source> Sources { get; set; } = new List<Source>();
}
