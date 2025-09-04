using System.Text.Json;
using DotNext;
using OpenAI;
using OpenAI.Responses;
using PodMD.Api.DTOs;
using PodMD.Api.DTOs.Schemas;
using PodMD.Api.Models;

#pragma warning disable OPENAI001

namespace PodMD.Api.Services;

public interface IDiagnosisService
{
    Task<Result<TroubleshootingResponse>> AskAsync(string logs, List<Guid> kbGuids,
        CancellationToken ct = default);

    Task<Result<TroubleshootingResponse>> AskAsync(Configuration config, string logs,
        List<Guid> kbGuids, CancellationToken ct = default);
}

public class RagDiagnosisService : IDiagnosisService
{
    private readonly OpenAIResponseClient openAIResponseClient;
    private readonly IKnowledgeBaseService _knowledgeBaseService;

    public RagDiagnosisService(OpenAIClient openAIClient, IKnowledgeBaseService kbService)
    {
        openAIResponseClient = openAIClient.GetOpenAIResponseClient("gpt-4o");
        _knowledgeBaseService = kbService;
    }

    public async Task<Result<TroubleshootingResponse>> AskAsync(string logs, List<Guid> kbGuids,
        CancellationToken ct = default)
    {
        var options = new ResponseCreationOptions
        {
            Instructions = Prompts.TroubleshootingPrompt
        };

        var tool = ResponseTool.CreateFunctionTool("structured_output", "returns structured output",
            BinaryData.FromString(TroubleshootingResponseSchema.Value), true);

        options.Tools.Add(tool);

        var vectorStoreIds = new List<string>();
        foreach (var kbGuid in kbGuids)
        {
            var kb = await _knowledgeBaseService.GetByGuidAsync(kbGuid);
            if (kb is not null) vectorStoreIds.Add(kb.OpenAIVectorStoreId);
        }

        if (vectorStoreIds.Count != 0)
            options.Tools.Add(ResponseTool.CreateFileSearchTool(vectorStoreIds));

        OpenAIResponse response = await openAIResponseClient.CreateResponseAsync(logs, options, ct);

        var responseItem = (FunctionCallResponseItem)response.OutputItems.First();

        try
        {
            var res = JsonSerializer.Deserialize<TroubleshootingResponse>(responseItem.FunctionArguments);

            return res is null
                ? Result.FromException<TroubleshootingResponse>(new JsonException("Deserialized value is null"))
                : Result.FromValue(res);
        }
        catch (JsonException ex)
        {
            return Result.FromException<TroubleshootingResponse>(ex);
        }
    }

    public async Task<Result<TroubleshootingResponse>> AskAsync(Configuration config, string logs,
        List<Guid> kbGuids,
        CancellationToken ct = default)
    {
        kbGuids.AddRange(config.KnowledgeBases.Select(kb => kb.Guid));

        return await AskAsync(logs, kbGuids, ct);
    }
}