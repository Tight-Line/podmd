using PodMD.Api.Authentication;
using PodMD.Api.DTOs;
using PodMD.Api.Requests;
using PodMD.Api.Services;

namespace PodMD.Api.Endpoints;

public static class DiagnoseEndpoints
{
    public static void MapDiagnoseEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/diagnose").WithTags("Diagnose");

        group.MapPost("/cluster/{configGuid:guid}/pod",
                async (IClusterService clusterService, IDiagnosisService ragDiagnosisService, Guid configGuid,
                    DiagnoseClusterPodRequest request) =>
                {
                    var cluster = await clusterService.GetByGuidAsync(configGuid);
                    if (cluster is null) return Results.NotFound();

                    var logs = await clusterService.GetLogsAsync(cluster, request.Namespace,
                        request.Pod, request.Tail, request.SinceTime);

                    var result =
                        await ragDiagnosisService.AskAsync(cluster, logs.Value,
                            (request.KnowledgeBaseGuids ?? Array.Empty<Guid>()).ToList());
                    return result.IsSuccessful ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
                })
            .WithName("DiagnoseConfiguredClusterPod")
            .Produces<TroubleshootingResponse>()
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();

        group.MapPost("/pod/adhoc",
                async (IClusterService clusterService, IDiagnosisService ragDiagnosisService,
                    AdhocDiagnosePodRequest request) =>
                {
                    var logs = await clusterService.GetLogsAsync(request.Host, request.Token, request.Namespace,
                        request.Pod, request.Tail, request.SinceTime);

                    var result = await ragDiagnosisService.AskAsync(logs.Value,
                        (request.KnowledgeBaseGuids ?? Array.Empty<Guid>()).ToList());
                    return result.IsSuccessful ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
                })
            .WithName("AdhocDiagnosePod")
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
}