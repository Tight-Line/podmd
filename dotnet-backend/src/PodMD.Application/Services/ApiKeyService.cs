using PodMD.Application.Dtos;
using PodMD.Application.Interfaces;
using PodMD.Domain.Entities;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PodMD.Application.Services;

public class ApiKeyService : IApiKeyService
{
    private readonly IApiKeyRepository _apiKeyRepository;

    public ApiKeyService(IApiKeyRepository apiKeyRepository)
    {
        _apiKeyRepository = apiKeyRepository;
    }

    public async Task<CreateApiKeyResponse> CreateApiKeyAsync(string userId, CreateApiKeyRequest request)
    {
        // Validate permissions JSON if provided
        if (!string.IsNullOrEmpty(request.Permissions))
        {
            try
            {
                JsonDocument.Parse(request.Permissions);
            }
            catch (JsonException)
            {
                throw new ArgumentException("Permissions must be valid JSON.");
            }
        }

        // Generate a new API key
        var apiKey = GenerateApiKey();
        var hashedKey = HashApiKey(apiKey);

        // Ensure uniqueness of hashed key (extremely unlikely collision but good practice)
        var existingKey = await _apiKeyRepository.ExistsAsync(hashedKey);
        var attempts = 0;
        while (existingKey && attempts < 5)
        {
            apiKey = GenerateApiKey();
            hashedKey = HashApiKey(apiKey);
            existingKey = await _apiKeyRepository.ExistsAsync(hashedKey);
            attempts++;
        }

        if (existingKey)
        {
            throw new InvalidOperationException("Failed to generate unique API key.");
        }

        var apiKeyEntity = new ApiKey
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            HashedKey = hashedKey,
            Permissions = request.Permissions,
            UsageLimit = request.UsageLimit,
            UsageCount = 0,
            Status = "active",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _apiKeyRepository.AddAsync(apiKeyEntity);

        return new CreateApiKeyResponse(
            apiKeyEntity.Id,
            apiKey,
            apiKeyEntity.Permissions,
            apiKeyEntity.UsageLimit
        );
    }

    public async Task<ApiKeyDto?> GetByIdAsync(Guid id, string userId)
    {
        var apiKey = await _apiKeyRepository.GetByIdAsync(id);
        if (apiKey == null || apiKey.UserId != userId)
        {
            return null;
        }

        return MapToDto(apiKey);
    }

    public async Task<IEnumerable<ApiKeyDto>> GetAllByUserIdAsync(string userId)
    {
        var apiKeys = await _apiKeyRepository.GetByUserIdAsync(userId);
        return apiKeys.Select(MapToDto);
    }

    public async Task<ApiKeyDto?> UpdateApiKeyAsync(Guid id, string userId, UpdateApiKeyRequest request)
    {
        var apiKey = await _apiKeyRepository.GetByIdAsync(id);
        if (apiKey == null || apiKey.UserId != userId)
        {
            return null;
        }

        // Validate permissions JSON if provided
        if (!string.IsNullOrEmpty(request.Permissions))
        {
            try
            {
                JsonDocument.Parse(request.Permissions);
            }
            catch (JsonException)
            {
                throw new ArgumentException("Permissions must be valid JSON.");
            }
        }

        // Update fields
        if (!string.IsNullOrEmpty(request.Permissions))
        {
            apiKey.Permissions = request.Permissions;
        }
        if (request.UsageLimit.HasValue)
        {
            apiKey.UsageLimit = request.UsageLimit;
        }
        if (!string.IsNullOrEmpty(request.Status))
        {
            if (request.Status != "active" && request.Status != "revoked")
            {
                throw new ArgumentException("Status must be 'active' or 'revoked'.");
            }
            apiKey.Status = request.Status;
            if (request.Status == "revoked" && !apiKey.RevokedAt.HasValue)
            {
                apiKey.RevokedAt = DateTimeOffset.UtcNow;
            }
        }

        apiKey.UpdatedAt = DateTimeOffset.UtcNow;

        await _apiKeyRepository.UpdateAsync(apiKey);

        return MapToDto(apiKey);
    }

    public async Task DeleteApiKeyAsync(Guid id, string userId)
    {
        var apiKey = await _apiKeyRepository.GetByIdAsync(id);
        if (apiKey != null && apiKey.UserId == userId)
        {
            await _apiKeyRepository.DeleteAsync(apiKey);
        }
    }

    private static string GenerateApiKey()
    {
        // 32 bytes = 256 bits of entropy
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static string HashApiKey(string apiKey)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(apiKey);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }

    private static ApiKeyDto MapToDto(ApiKey apiKey)
    {
        return new ApiKeyDto(
            apiKey.Id,
            apiKey.UserId,
            apiKey.Permissions,
            apiKey.UsageLimit,
            apiKey.UsageCount,
            apiKey.Status,
            apiKey.CreatedAt,
            apiKey.LastUsedAt
        );
    }
}
