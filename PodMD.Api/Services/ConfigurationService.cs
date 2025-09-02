using DotNext;
using Microsoft.EntityFrameworkCore;
using PodMD.Api.Database;
using PodMD.Api.Models;

namespace PodMD.Api.Services;

public interface IConfigurationService
{
    Task<Configuration?> GetByGuidAsync(Guid guid);
    Task<Result<bool>> LinkAsync(Configuration configuration, KnowledgeBase knowledgeBase);
    Task<Result<bool>> UnlinkAsync(Configuration configuration, KnowledgeBase knowledgeBase);
}

public class ConfigurationService(AppDbContext dbContext, IAuthenticatedApiKeyService authenticatedApiKeyService)
    : IConfigurationService
{
    public async Task<Configuration?> GetByGuidAsync(Guid guid)
    {
        return await dbContext.Configurations
            .Include(c => c.KnowledgeBases)
            .FirstOrDefaultAsync(c => c.Guid == guid && c.ApiKeyId == authenticatedApiKeyService.ApiKeyId);
    }

    public async Task<Result<bool>> LinkAsync(Configuration configuration, KnowledgeBase knowledgeBase)
    {
        configuration.KnowledgeBases.Add(knowledgeBase);
        await dbContext.SaveChangesAsync();
        return Result.FromValue(true);
    }

    public async Task<Result<bool>> UnlinkAsync(Configuration configuration, KnowledgeBase knowledgeBase)
    {
        configuration.KnowledgeBases.Remove(knowledgeBase);
        await dbContext.SaveChangesAsync();
        return Result.FromValue(true);
    }
}