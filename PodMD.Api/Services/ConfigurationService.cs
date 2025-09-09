using DotNext;
using Microsoft.EntityFrameworkCore;
using PodMD.Api.Database;
using PodMD.Api.Models;

namespace PodMD.Api.Services;

public interface IConfigurationService
{
    Task<Result<List<Configuration>>> GetAllAsync();
    Task<Configuration?> GetByGuidAsync(Guid guid);
    Task<Result<bool>> DeleteAsync(Guid guid);
    Task<Result<List<KnowledgeBase>>> GetAvailableKnowledgeBases(Guid guid);
    Task<Result<bool>> LinkAsync(Configuration configuration, KnowledgeBase knowledgeBase);
    Task<Result<bool>> UnlinkAsync(Configuration configuration, KnowledgeBase knowledgeBase);
}

public class ConfigurationService(AppDbContext dbContext, IAuthenticatedApiKeyService authenticatedApiKeyService)
    : IConfigurationService
{
    public async Task<Result<List<Configuration>>> GetAllAsync()
    {
        try
        {
            var configs = await dbContext.Configurations
                .Where(c => c.ApiKeyId == authenticatedApiKeyService.ApiKeyId)
                .ToListAsync();

            return Result.FromValue(configs);
        }
        catch (Exception ex)
        {
            return Result.FromException<List<Configuration>>(ex);
        }
    }

    public async Task<Configuration?> GetByGuidAsync(Guid guid)
    {
        return await dbContext.Configurations
            .Include(c => c.KnowledgeBases)
            .FirstOrDefaultAsync(c => c.Guid == guid && c.ApiKeyId == authenticatedApiKeyService.ApiKeyId);
    }

    public async Task<Result<bool>> DeleteAsync(Guid guid)
    {
        var config = await dbContext.Configurations.FirstOrDefaultAsync(c =>
            c.Guid == guid && c.ApiKeyId == authenticatedApiKeyService.ApiKeyId);

        if (config is null)
            return Result.FromException<bool>(new KeyNotFoundException($"Configuration with Guid {guid} not found"));

        dbContext.Configurations.Remove(config);
        await dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<Result<List<KnowledgeBase>>> GetAvailableKnowledgeBases(Guid guid)
    {
        var availableKbs = await dbContext.Set<KnowledgeBase>()
            .Where(kb => !kb.Configurations.Any(link => link.Guid == guid))
            .ToListAsync();

        return Result.FromValue(availableKbs);
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