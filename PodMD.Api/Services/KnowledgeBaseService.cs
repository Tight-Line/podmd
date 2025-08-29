using DotNext;
using Microsoft.EntityFrameworkCore;
using PodMD.Api.Database;
using PodMD.Api.Models;

namespace PodMD.Api.Services;

public interface IKnowledgeBaseService
{
    Task<Result<List<KnowledgeBase>>> GetAllAsync();
    Task<KnowledgeBase?> GetByGuidAsync(Guid guid);
    Task<Result<KnowledgeBase>> CreateAsync(string host, string token);
    Task<Result<bool>> DeleteAsync(KnowledgeBase knowledgeBase);
}

public class KnowledgeBaseService(
    AppDbContext dbContext,
    IAuthenticatedApiKeyService authenticatedApiKeyService,
    IOpenAIService openAiService)
    : IKnowledgeBaseService
{
    public async Task<Result<List<KnowledgeBase>>> GetAllAsync()
    {
        try
        {
            var knowledgeBases = await dbContext.KnowledgeBases
                .Where(c => c.ApiKeyId == authenticatedApiKeyService.ApiKeyId)
                .ToListAsync();

            return Result.FromValue(knowledgeBases);
        }
        catch (Exception ex)
        {
            return Result.FromException<List<KnowledgeBase>>(ex);
        }
    }

    public async Task<KnowledgeBase?> GetByGuidAsync(Guid guid)
    {
        return await dbContext.KnowledgeBases.Include(kb => kb.Resources).FirstOrDefaultAsync(kb =>
            kb.Guid == guid && kb.ApiKeyId == authenticatedApiKeyService.ApiKeyId);
    }

    public async Task<Result<KnowledgeBase>> CreateAsync(string name, string description)
    {
        var result = await openAiService.CreateVectorStoreAsync();
        var knowledgeBase = new KnowledgeBase
        {
            Guid = Guid.NewGuid(),
            Name = name,
            Description = description,
            OpenAIVectorStoreId = result.VectorStoreId,
            CreatedAt = DateTimeOffset.UtcNow,
            ApiKeyId = authenticatedApiKeyService.ApiKeyId
        };

        dbContext.KnowledgeBases.Add(knowledgeBase);
        await dbContext.SaveChangesAsync();

        return knowledgeBase;
    }

    public async Task<Result<bool>> DeleteAsync(KnowledgeBase knowledgeBase)
    {
        dbContext.KnowledgeBases.Remove(knowledgeBase);
        await dbContext.SaveChangesAsync();

        return true;
    }
}