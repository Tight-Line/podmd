using System.ComponentModel.DataAnnotations;

namespace PodMD.Application.Dtos;

public record SourceCreateDto(
    [Required]
    [StringLength(100)]
    string Name,

    [Required]
    [Url]
    string Server,

    string? Instructions,

    string? ResponseFormat
);

public record SourceReadDto(
    Guid Id,
    string Name,
    string Type,
    string Server,
    string? Instructions,
    string? ResponseFormat,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
