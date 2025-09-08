using DotNext;
using Microsoft.EntityFrameworkCore;
using PodMD.Api.Database;
using PodMD.Api.Models;

namespace PodMD.Api.Services;

public interface IResourceService
{
    Task<Resource?> GetByGuidAsync(Guid guid);
    Task<Result<Resource>> UploadAsync(KnowledgeBase knowledgeBase, string fileName, Stream stream);

    Task<Result<Resource>> ReplaceAsync(KnowledgeBase knowledgeBase, Resource resource, string fileName,
        Stream stream);

    Task<Result<bool>> DeleteAsync(Resource resource);
}

public class ResourceService(
    AppDbContext dbContext,
    IAuthenticatedApiKeyService authenticatedApiKeyService,
    IOpenAIService openAiService) : IResourceService
{
    public async Task<Resource?> GetByGuidAsync(Guid guid)
    {
        return await dbContext.Resources.FirstOrDefaultAsync(kb =>
            kb.Guid == guid && kb.ApiKeyId == authenticatedApiKeyService.ApiKeyId);
    }

    public async Task<Result<Resource>> UploadAsync(KnowledgeBase knowledgeBase, string fileName, Stream stream)
    {
        var openAIFile = await openAiService.UploadFileAsync(stream, fileName);

        var resource = new Resource()
        {
            FileName = fileName,
            Guid = Guid.NewGuid(),
            OpenAIFileId = openAIFile.Value.Id,
            CreatedAt = DateTimeOffset.UtcNow,
            ApiKeyId = authenticatedApiKeyService.ApiKeyId,
            KnowledgeBaseId = knowledgeBase.Id
        };

        await openAiService.AddFileToVectorStoreAsync(knowledgeBase.OpenAIVectorStoreId, openAIFile.Value.Id);

        knowledgeBase.Resources.Add(resource);

        await dbContext.SaveChangesAsync();

        return resource;
    }

    public async Task<Result<Resource>> ReplaceAsync(KnowledgeBase knowledgeBase, Resource resource,
        string fileName,
        Stream stream)
    {
        await openAiService.DeleteFileAsync(resource.OpenAIFileId);

        var openAIFile = await openAiService.UploadFileAsync(stream, fileName);

        resource.OpenAIFileId = openAIFile.Value.Id;
        resource.FileName = fileName;

        await openAiService.AddFileToVectorStoreAsync(knowledgeBase.OpenAIVectorStoreId, openAIFile.Value.Id);

        dbContext.Resources.Update(resource);

        await dbContext.SaveChangesAsync();

        return resource;
    }

    public async Task<Result<bool>> DeleteAsync(Resource resource)
    {
        await openAiService.DeleteFileAsync(resource.OpenAIFileId);

        dbContext.Resources.Remove(resource);
        await dbContext.SaveChangesAsync();

        return true;
    }
}