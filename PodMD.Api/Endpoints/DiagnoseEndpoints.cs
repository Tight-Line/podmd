using System.Globalization;
using System.Text.Json;
using DotNext;
using PodMD.Api.Authentication;
using PodMD.Api.DTOs;
using PodMD.Api.Models;
using PodMD.Api.Requests;
using PodMD.Api.Services;

namespace PodMD.Api.Endpoints;

public static class DiagnoseEndpoints
{
    public static void MapDiagnoseEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/diagnose").WithTags("Diagnose");

        group.MapPost("/{configGuid:guid}",
                async (IConfigurationService configService, IClusterService clusterService,
                    IDiagnosisService ragDiagnosisService, Guid configGuid,
                    Dictionary<string, JsonElement> parameters) =>
                {
                    var config = await configService.GetByGuidAsync(configGuid);
                    if (config is null) return Results.NotFound();

                    var logs = await GetLogsAsync(config, parameters, clusterService);

                    var result = await ragDiagnosisService.AskAsync(config, logs.Value, Array.Empty<Guid>().ToList());
                    return result.IsSuccessful ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
                })
            .WithName("Diagnose")
            .Produces<TroubleshootingResponse>()
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();

        group.MapPost("/logs",
                async (IDiagnosisService ragDiagnosisService, DiagnoseLogsRequest request) =>
                {
                    var result = await ragDiagnosisService.AskAsync(request.Logs,
                        (request.KnowledgeBaseGuids ?? Array.Empty<Guid>()).ToList());
                    return result.IsSuccessful ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
                })
            .WithName("DiagnoseLogs")
            .Produces<TroubleshootingResponse>()
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();
    }

    private static async Task<Result<string>> GetLogsAsync(Configuration config,
        Dictionary<string, JsonElement> parameters,
        IClusterService clusterService)
    {
        switch (config)
        {
            case Cluster cluster:
            {
                if (!parameters.TryGetValue("namespace", out var nsEl) ||
                    nsEl.ValueKind != JsonValueKind.String ||
                    string.IsNullOrWhiteSpace(nsEl.GetString()))
                    return Result.FromException<string>(
                        new JsonException("namespace is required and must be a non-empty string."));
                var ns = nsEl.GetString()!;

                if (!parameters.TryGetValue("pod", out var podEl) ||
                    podEl.ValueKind != JsonValueKind.String ||
                    string.IsNullOrWhiteSpace(podEl.GetString()))
                    return Result.FromException<string>(
                        new JsonException("pod is required and must be a non-empty string."));
                var pod = podEl.GetString()!;

                int? tail = null;
                if (parameters.TryGetValue("tail", out var tailEl) &&
                    tailEl.ValueKind != JsonValueKind.Null &&
                    tailEl.ValueKind != JsonValueKind.Undefined)
                    try
                    {
                        if (tailEl.ValueKind == JsonValueKind.Number)
                            tail = tailEl.GetInt32();
                        else if (tailEl.ValueKind == JsonValueKind.String &&
                                 int.TryParse(tailEl.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture,
                                     out var t))
                            tail = t;
                        else
                            return Result.FromException<string>(new JsonException("tail must be a number (int)."));
                    }
                    catch
                    {
                        return Result.FromException<string>(new JsonException("tail must fit into Int32."));
                    }

                DateTimeOffset? sinceTime = null;
                if (parameters.TryGetValue("sinceTime", out var sinceEl) &&
                    sinceEl.ValueKind != JsonValueKind.Null &&
                    sinceEl.ValueKind != JsonValueKind.Undefined)
                {
                    if (sinceEl.ValueKind != JsonValueKind.String ||
                        !DateTimeOffset.TryParse(sinceEl.GetString(), CultureInfo.InvariantCulture,
                            DateTimeStyles.RoundtripKind, out var dto))
                        return Result.FromException<string>(new JsonException("sinceTime must be an ISO-8601 string."));
                    sinceTime = dto;
                }

                return await clusterService.GetLogsAsync(cluster, ns, pod, tail, sinceTime);
            }
            default:
            {
                return Result.FromException<string>(new JsonException("Unsupported configuration"));
            }
        }
    }
}