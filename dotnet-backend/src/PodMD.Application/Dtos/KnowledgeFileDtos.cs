using System.ComponentModel.DataAnnotations;

namespace PodMD.Application.Dtos;

// Upload DTO used by service layer (file data handled separately)
public record CreateKnowledgeFileDto(
    Guid KnowledgeBaseId,
    string FileName,
    string ContentType,
    long FileSize,
    Stream FileStream
);

// Replace DTO for file replacement (file data handled separately)
public record UpdateKnowledgeFileDto(
    string? FileName,
    string ContentType,
    long FileSize,
    Stream FileStream
);

public record KnowledgeFileDto(
    Guid Id,
    Guid KnowledgeBaseId,
    string FileName,
    string ContentType,
    long FileSize,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
