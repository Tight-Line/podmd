using DotNext;
using Microsoft.EntityFrameworkCore;
using PodMD.Api.Database;
using PodMD.Api.DTOs;
using PodMD.Api.Models;

namespace PodMD.Api.Services;

public interface IApiKeyService
{
    Task<Result<ApiKeyDto>> CreateAsync();
    Task<ApiKey?> GetByLookupNameAsync(string lookupName);
}

public class ApiKeyService(AppDbContext dbContext, IHashingService hashingService) : IApiKeyService
{
    public async Task<Result<ApiKeyDto>> CreateAsync()
    {
        var lookupName = Guid.NewGuid().ToString("N");
        var secret = Guid.NewGuid().ToString("N");
        var hash = hashingService.Hash(secret);

        var apiKey = new ApiKey
        {
            LookupName = lookupName,
            SecretHash = hash,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.ApiKeys.Add(apiKey);
        await dbContext.SaveChangesAsync();

        return new ApiKeyDto($"{lookupName}-{secret}", apiKey);
    }

    public async Task<ApiKey?> GetByLookupNameAsync(string lookupName)
    {
        return await dbContext.ApiKeys.FirstOrDefaultAsync(k => k.LookupName == lookupName);
    }
}