using System.Text.Json;
using DotNext;
using DotNext.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using OpenAI;
using OpenAI.Assistants;
using OpenAI.Files;
using OpenAI.VectorStores;
using PodMD.Api.Database;
using PodMD.Api.DTOs;
using PodMD.Api.DTOs.Schemas;
using PodMD.Api.Models;

#pragma warning disable OPENAI001

namespace PodMD.Api.Services;

public interface IRagService
{
    Task<Result<Cluster>> CreateOpenAIAssistantAsync(Cluster cluster);
    Task<Result<List<RagResource>>> CreateOpenAIAssistantAsync(Cluster cluster, IFormFileCollection files);

    Task<Result<TroubleshootingResponse>> AskAsync(string assistantId, string logs,
        CancellationToken ct = default);

    Task<RagResource?> GetByGuid(Guid guid);
    Task<Result<RagResource>> UploadAsync(string fileName, Stream stream);
    Task<Result<bool>> LinkAsync(Cluster cluster, RagResource ragResource);
}

public class RagService : IRagService
{
    private readonly AssistantClient _assistantClient;
    private readonly OpenAIFileClient _openAIFileClient;
    private readonly VectorStoreClient _vectorStoreClient;
    private readonly AppDbContext _dbContext;

    public RagService(OpenAIClient openAIClient, AppDbContext dbContext)
    {
        _dbContext = dbContext;
        _assistantClient = openAIClient.GetAssistantClient();
        _openAIFileClient = openAIClient.GetOpenAIFileClient();
        _vectorStoreClient = openAIClient.GetVectorStoreClient();
    }

    public async Task<Result<Cluster>> CreateOpenAIAssistantAsync(Cluster cluster)
    {
        var tool = new FunctionToolDefinition
        {
            FunctionName = "ParseTroubleshootingLogs",
            Description = "Parse pod logs and return structured troubleshooting JSON.",
            StrictParameterSchemaEnabled = true,
            Parameters = new BinaryData(TroubleshootingResponseSchema.Value)
        };

        var assistant = await _assistantClient.CreateAssistantAsync("gpt-4o", new AssistantCreationOptions()
        {
            Name = "Kubernetes troubleshooting assistant",
            Instructions = Prompts.TroubleshootingPrompt,
            Tools =
            {
                new FileSearchToolDefinition(),
                tool
            },
            ToolResources = new ToolResources
            {
                FileSearch = new FileSearchToolResources
                {
                    NewVectorStores =
                    {
                        new VectorStoreCreationHelper()
                    }
                }
            }
        });

        cluster.OpenAIAssistantId = assistant.Value.Id;
        _dbContext.Clusters.Update(cluster);
        await _dbContext.SaveChangesAsync();

        return cluster;
    }

    public async Task<Result<List<RagResource>>> CreateOpenAIAssistantAsync(Cluster cluster, IFormFileCollection files)
    {
        var ragResources = new List<RagResource>();
        foreach (var file in files)
        {
            await using var stream = file.OpenReadStream();
            var ragResource = await UploadAsync(file.FileName, stream);
            ragResources.Add(ragResource.Value);
        }

        var tool = new FunctionToolDefinition
        {
            FunctionName = "ParseTroubleshootingLogs",
            Description = "Parse pod logs and return structured troubleshooting JSON.",
            StrictParameterSchemaEnabled = true,
            Parameters = new BinaryData(TroubleshootingResponseSchema.Value)
        };

        var assistant = await _assistantClient.CreateAssistantAsync("gpt-4o", new AssistantCreationOptions()
        {
            Name = "Kubernetes troubleshooting assistant",
            Instructions = Prompts.TroubleshootingPrompt,
            Tools =
            {
                new FileSearchToolDefinition(),
                tool
            },
            ToolResources = new ToolResources
            {
                FileSearch = new FileSearchToolResources
                {
                    NewVectorStores =
                    {
                        new VectorStoreCreationHelper(ragResources.Select(r => r.OpenAIFileId))
                    }
                }
            }
        });

        cluster.RagResources.AddAll(ragResources);
        cluster.OpenAIAssistantId = assistant.Value.Id;
        _dbContext.Clusters.Update(cluster);
        await _dbContext.SaveChangesAsync();

        return ragResources;
    }


    public async Task<Result<TroubleshootingResponse>> AskAsync(string assistantId, string logs,
        CancellationToken ct = default)
    {
        ThreadRun run = await _assistantClient.CreateThreadAndRunAsync(assistantId,
            new ThreadCreationOptions
            {
                InitialMessages = { logs }
            }, cancellationToken: ct);

        do
        {
            await Task.Delay(TimeSpan.FromSeconds(1), ct);
            run = await _assistantClient.GetRunAsync(run.ThreadId, run.Id, ct);
            if (run.Status == RunStatus.RequiresAction)
            {
                var toolOutputs = run.RequiredActions.Select(x =>
                    new ToolOutput(x.ToolCallId, JsonSerializer.Serialize(x.FunctionArguments))).ToList();
                run = await _assistantClient.SubmitToolOutputsToRunAsync(run.ThreadId, run.Id, toolOutputs, ct);
            }
        } while (!run.Status.IsTerminal);

        var messages = _assistantClient.GetMessages(run.ThreadId, cancellationToken: ct);

        var json = messages
            .SelectMany(m => m.Content)
            .Select(c => c.Text)
            .FirstOrDefault(t => !string.IsNullOrEmpty(t));

        if (string.IsNullOrWhiteSpace(json))
            return Result.FromException<TroubleshootingResponse>(new Exception("Response from OpenAI is empty"));

        var cleaned = json.Trim()
            .Trim('`')
            .Replace("json", "", StringComparison.OrdinalIgnoreCase)
            .Trim();

        try
        {
            var res = JsonSerializer.Deserialize<TroubleshootingResponse>(cleaned);

            return res is null
                ? Result.FromException<TroubleshootingResponse>(new JsonException("Deserialized value is null"))
                : Result.FromValue(res);
        }
        catch (JsonException ex)
        {
            return Result.FromException<TroubleshootingResponse>(ex);
        }
    }

    public async Task<RagResource?> GetByGuid(Guid guid)
    {
        return await _dbContext.RagResources.FirstOrDefaultAsync(r => r.Guid == guid);
    }

    public async Task<Result<RagResource>> UploadAsync(string fileName, Stream stream)
    {
        var openAIFile = await _openAIFileClient.UploadFileAsync(stream, fileName, FileUploadPurpose.Assistants);

        var ragResource = new RagResource()
        {
            Name = fileName,
            Guid = Guid.NewGuid(),
            OpenAIFileId = openAIFile.Value.Id
        };

        _dbContext.RagResources.Add(ragResource);
        await _dbContext.SaveChangesAsync();

        return ragResource;
    }

    public async Task<Result<bool>> LinkAsync(Cluster cluster, RagResource ragResource)
    {
        var assistant = await _assistantClient.GetAssistantAsync(cluster.OpenAIAssistantId);
        var vsId = assistant.Value.ToolResources.FileSearch.VectorStoreIds.FirstOrDefault();

        await _vectorStoreClient.AddFileToVectorStoreAsync(vsId, ragResource.OpenAIFileId, true);

        cluster.RagResources.Add(ragResource);
        await _dbContext.SaveChangesAsync();

        return Result.FromValue(true);
    }

    // public async Task<Result<bool>> AddFileToAssistant(string fileFileName, byte[] fileBytes)
    // {
    //     var document = BinaryData.FromBytes(fileBytes);
    //
    //     var vsId = _assistant.ToolResources.FileSearch.VectorStoreIds.FirstOrDefault();
    //
    //     OpenAIFile documentationFile = await _openAIFileClient.UploadFileAsync(
    //         document,
    //         fileFileName,
    //         FileUploadPurpose.Assistants);
    //
    //
    //     await _vectorStoreClient.AddFileToVectorStoreAsync(vsId, documentationFile.Id, true);
    //
    //     return true;
    // }
}