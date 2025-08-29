using System.Text.Json;
using DotNext;
using OpenAI;
using OpenAI.Responses;
using PodMD.Api.DTOs;
using PodMD.Api.Models;

#pragma warning disable OPENAI001

namespace PodMD.Api.Services;

public interface IResponseRagService
{
    Task<Result<TroubleshootingResponse>> AskAsync(Cluster cluster, string logs, CancellationToken ct = default);
}

public class ResponseRagService : IResponseRagService
{
    private readonly OpenAIResponseClient openAIResponseClient;

    public ResponseRagService(OpenAIClient openAIClient)
    {
        openAIResponseClient = openAIClient.GetOpenAIResponseClient("gpt-4o");
    }

    public async Task<Result<TroubleshootingResponse>> AskAsync(Cluster cluster, string logs,
        CancellationToken ct = default)
    {
        var options = new ResponseCreationOptions()
        {
            Instructions = Prompts.TroubleshootingPrompt
        };

        if (cluster.KnowledgeBases.Count != 0)
        {
            var vectorStoreIds = cluster.KnowledgeBases.Select(kb => kb.OpenAIVectorStoreId);
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