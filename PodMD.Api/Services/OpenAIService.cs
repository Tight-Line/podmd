using System.ClientModel;
using OpenAI;
using OpenAI.Files;
using OpenAI.VectorStores;

#pragma warning disable OPENAI001

namespace PodMD.Api.Services;

public interface IOpenAIService
{
    Task<CreateVectorStoreOperation> CreateVectorStoreAsync();
    Task<ClientResult<OpenAIFile>> UploadFileAsync(Stream stream, string fileName);
    Task<AddFileToVectorStoreOperation> AddFileToVectorStoreAsync(string vectorStoreId, string fileId);
    Task<ClientResult<FileDeletionResult>> DeleteFileAsync(string resourceOpenAiFileId);
}

public class OpenAIService : IOpenAIService
{
    private readonly OpenAIFileClient _openAIFileClient;
    private readonly VectorStoreClient _vectorStoreClient;

    public OpenAIService(OpenAIClient openAIClient)
    {
        _openAIFileClient = openAIClient.GetOpenAIFileClient();
        _vectorStoreClient = openAIClient.GetVectorStoreClient();
    }

    public async Task<CreateVectorStoreOperation> CreateVectorStoreAsync()
    {
        return await _vectorStoreClient.CreateVectorStoreAsync(true);
    }

    public async Task<ClientResult<OpenAIFile>> UploadFileAsync(Stream stream, string fileName)
    {
        return await _openAIFileClient.UploadFileAsync(stream, fileName, FileUploadPurpose.Assistants);
    }

    public async Task<AddFileToVectorStoreOperation> AddFileToVectorStoreAsync(string vectorStoreId, string fileId)
    {
        return await _vectorStoreClient.AddFileToVectorStoreAsync(vectorStoreId, fileId, true);
    }

    public async Task<ClientResult<FileDeletionResult>> DeleteFileAsync(string resourceOpenAiFileId)
    {
        return await _openAIFileClient.DeleteFileAsync(resourceOpenAiFileId);
    }
}