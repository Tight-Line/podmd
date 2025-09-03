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
    Task<Result<TroubleshootingResponse>> AskAsync(Configuration config, string logs, CancellationToken ct = default);

    Task<Result<TroubleshootingResponse>> AskAsync(IEnumerable<string> vectorStoreIds, string logs,
        CancellationToken ct = default);
}

public class RagDiagnosisService : IDiagnosisService
{
    private readonly OpenAIResponseClient openAIResponseClient;

    public RagDiagnosisService(OpenAIClient openAIClient)
    {
        openAIResponseClient = openAIClient.GetOpenAIResponseClient("gpt-4o");
    }

    public async Task<Result<TroubleshootingResponse>> AskAsync(IEnumerable<string> vectorStoreIds, string logs,
        CancellationToken ct = default)
    {
        var options = new ResponseCreationOptions
        {
            Instructions = Prompts.TroubleshootingPrompt
        };

        var tool = ResponseTool.CreateFunctionTool("structured_output", "returns structured output",
            BinaryData.FromString(TroubleshootingResponseSchema.Value), true);

        options.Tools.Add(tool);

        if (vectorStoreIds.Any())
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
        CancellationToken ct = default)
    {
        var options = new ResponseCreationOptions()
        {
            Instructions = Prompts.TroubleshootingPrompt
        };

        if (config.KnowledgeBases.Count != 0)
        {
            var vectorStoreIds = config.KnowledgeBases.Select(kb => kb.OpenAIVectorStoreId);
            options.Tools.Add(ResponseTool.CreateFileSearchTool(vectorStoreIds));
        }

        OpenAIResponse response = await openAIResponseClient.CreateResponseAsync(logs, options, ct);

        var json = response.GetOutputText();

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
}