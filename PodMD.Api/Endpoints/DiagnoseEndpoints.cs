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

        group.MapPost("/cluster/{configGuid:guid}",
                async (IClusterService clusterService, IDiagnosisService ragDiagnosisService, Guid configGuid,
                    DiagnoseClusterRequest request) =>
                {
                    var cluster = await clusterService.GetByGuidAsync(configGuid);
                    if (cluster is null) return Results.NotFound();

                    var logs = await clusterService.GetLogsAsync(cluster, request.Namespace,
                        request.Pod, request.Tail, request.SinceTime);

                    var result = await ragDiagnosisService.AskAsync(cluster, logs.Value);
                    return result.IsSuccessful ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
                })
            .WithName("DiagnoseCluster")
            .Produces<TroubleshootingResponse>()
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();
    }
}