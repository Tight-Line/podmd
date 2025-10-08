using System.ComponentModel.DataAnnotations;

namespace PodMD.Application.Dtos;

public record CreateApiKeyRequest(
    string? Permissions,
    int? UsageLimit
);

public record CreateApiKeyResponse(
    Guid Id,
    string Key,
    string? Permissions,
    int? UsageLimit
);

public record UpdateApiKeyRequest(
    string? Permissions,
    int? UsageLimit,
    string? Status
);

public record ApiKeyDto(
    Guid Id,
    string UserId,
    string? Permissions,
    int? UsageLimit,
    int UsageCount,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastUsedAt
);
