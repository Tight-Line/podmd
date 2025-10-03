using System.ComponentModel.DataAnnotations;

namespace PodMD.Application.Dtos;

public record CreateKnowledgeBaseDto(
    [Required]
    [StringLength(100)]
    string Name,

    [StringLength(1000)]
    string? Description
);

public record UpdateKnowledgeBaseDto(
    Guid Id,

    [Required]
    [StringLength(100)]
    string Name,

    [StringLength(1000)]
    string? Description
);

public record KnowledgeBaseDto(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record KnowledgeBaseWithSourcesDto(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IEnumerable<SourceReadDto> Sources
);
