using PodMD.Api.Requests;
using PodMD.Api.Services;

namespace PodMD.Api.Endpoints;

public static class ClusterEndpoints
{
    public static void MapClusterEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/clusters");

        group.MapPost("/", async (IClusterService clusterService, CreateClusterRequest request) =>
        {
            var result = await clusterService.Create(request.Host, request.Token);
            return result.IsSuccessful
                ? Results.Ok(new { result.Value.Guid, result.Value.Host })
                : Results.BadRequest();
        });

        group.MapPost("/how-to-fix", async (IClusterService clusterService, HowToFixRequest request) =>
        {
            var cluster = await clusterService.GetByGuid(request.ClusterGuid);
            if (cluster is null) return Results.NotFound();

            var result = await clusterService.AnalyzeLogsAsync(cluster.Host, cluster.AccessToken, request.Namespace,
                request.Pod, request.Tail, request.SinceTime);
            return result.IsSuccessful ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });
    }
}