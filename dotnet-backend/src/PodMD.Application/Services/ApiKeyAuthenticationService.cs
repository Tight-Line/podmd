using PodMD.Application.Dtos;
using PodMD.Application.Interfaces;
using PodMD.Domain.Entities;
using System.Security.Cryptography;
using System.Text;

namespace PodMD.Application.Services;

public class ApiKeyAuthenticationService : IApiKeyAuthenticationService
{
    private readonly IApiKeyRepository _apiKeyRepository;

    public ApiKeyAuthenticationService(IApiKeyRepository apiKeyRepository)
    {
        _apiKeyRepository = apiKeyRepository;
    }

    public async Task<UserDto?> AuthenticateApiKeyAsync(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            return null;
        }

        // Hash the provided API key for database lookup
        var hashedKey = HashApiKey(apiKey);

        // Find the API key in the database
        var apiKeyEntity = await _apiKeyRepository.GetByHashedKeyAsync(hashedKey);
        if (apiKeyEntity == null)
        {
            return null;
        }

        // Check if the key is active
        if (apiKeyEntity.Status != "active")
        {
            return null;
        }

        // Check usage limit
        if (await IsUsageLimitExceededAsync(hashedKey))
        {
            return null;
        }

        // Increment usage count
        await IncrementUsageAsync(hashedKey);

        // Map to UserDto for authentication
        return new UserDto
        {
            Id = apiKeyEntity.UserId,
            FirstName = apiKeyEntity.User.FirstName,
            LastName = apiKeyEntity.User.LastName,
            Email = apiKeyEntity.User.Email!,
            CreatedAt = apiKeyEntity.User.CreatedAt,
            UpdatedAt = apiKeyEntity.User.UpdatedAt
        };
    }

    public async Task<bool> IsUsageLimitExceededAsync(string hashedKey)
    {
        var apiKey = await _apiKeyRepository.GetByHashedKeyAsync(hashedKey);
        if (apiKey == null || !apiKey.UsageLimit.HasValue)
        {
            return false; // No limit set
        }

        return apiKey.UsageCount >= apiKey.UsageLimit.Value;
    }

    public async Task IncrementUsageAsync(string hashedKey)
    {
        var apiKey = await _apiKeyRepository.GetByHashedKeyAsync(hashedKey);
        if (apiKey != null)
        {
            apiKey.UsageCount++;
            apiKey.LastUsedAt = DateTimeOffset.UtcNow;
            await _apiKeyRepository.UpdateAsync(apiKey);
        }
    }

    private static string HashApiKey(string apiKey)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(apiKey);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
