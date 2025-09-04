using PodMD.Api.Authentication;
using PodMD.Api.Requests;
using PodMD.Api.Responses;
using PodMD.Api.Services;

namespace PodMD.Api.Endpoints;

public static class ClusterEndpoints
{
    public static void MapClusterEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/configurations/clusters").WithTags("Cluster Configurations");

        group.MapGet("", async (IClusterService clusterService) =>
            {
                var result = await clusterService.GetAllAsync();
                return result.IsSuccessful
                    ? Results.Ok(result.Value.Select(c => new { c.Guid, c.Name, c.Host }))
                    : Results.BadRequest(result.Error);
            })
            .WithName("GetClusters")
            .Produces<IEnumerable<ClusterResponse>>()
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();

        group.MapPost("", async (IClusterService clusterService, CreateClusterRequest request) =>
            {
                var result =
                    await clusterService.CreateAsync(request.Name, request.Host, request.Token, request.CaCertPem);
                return result.IsSuccessful
                    ? Results.Ok(new { result.Value.Guid, result.Value.Name, result.Value.Host })
                    : Results.BadRequest(result.Error);
            })
            .WithName("RegisterCluster")
            .Produces<ClusterResponse>(StatusCodes.Status201Created)
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();

        group.MapPut("/{configGuid:guid}",
                async (IClusterService clusterService, Guid configGuid, UpdateClusterRequest request) =>
                {
                    var cluster = await clusterService.GetByGuidAsync(configGuid);
                    if (cluster is null) return Results.NotFound();

                    var result =
                        await clusterService.UpdateAsync(configGuid, request.Name, request.Host, request.Token,
                            request.CaCertPem);
                    return result.IsSuccessful
                        ? Results.Ok(new { result.Value.Guid, result.Value.Name, result.Value.Host })
                        : Results.BadRequest(result.Error);
                })
            .WithName("UpdateCluster")
            .Produces<ClusterResponse>()
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();

        group.MapDelete("/{configGuid:guid}", async (IClusterService clusterService, Guid configGuid) =>
            {
                var cluster = await clusterService.GetByGuidAsync(configGuid);
                if (cluster is null) return Results.NotFound();

                var result = await clusterService.DeleteAsync(configGuid);
                return result.IsSuccessful ? Results.NoContent() : Results.BadRequest(result.Error);
            })
            .WithName("UnregisterCluster")
            .Produces(StatusCodes.Status204NoContent)
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();

        group.MapPost("/test-connection", async (IClusterService clusterService, CreateClusterRequest request) =>
            {
                var result = await clusterService.TestConnectionAsync(request.Host, request.Token, request.CaCertPem);
                return result.IsSuccessful
                    ? Results.Ok(result.Value)
                    : Results.BadRequest(result.Error);
            })
            .WithName("TestClusterConnection")
            .Produces<bool>()
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();
    }
}