using PodMD.Api.Authentication;
using PodMD.Api.DTOs;
using PodMD.Api.Requests;
using PodMD.Api.Responses;
using PodMD.Api.Services;

namespace PodMD.Api.Endpoints;

public static class ClusterEndpoints
{
    public static void MapClusterEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/clusters").WithTags("Clusters");

        group.MapGet("", async (IClusterService clusterService) =>
            {
                var result = await clusterService.GetAllAsync();
                return result.IsSuccessful
                    ? Results.Ok(result.Value.Select(c => new { c.Guid, c.Host }))
                    : Results.BadRequest(result.Error);
            })
            .WithName("GetClusters")
            .Produces<IEnumerable<ClusterResponse>>()
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();

        group.MapPost("", async (IClusterService clusterService, CreateClusterRequest request) =>
            {
                var result = await clusterService.CreateAsync(request.Host, request.Token);
                return result.IsSuccessful
                    ? Results.Ok(new { result.Value.Guid, result.Value.Host })
                    : Results.BadRequest(result.Error);
            })
            .WithName("RegisterCluster")
            .Produces<ClusterResponse>(StatusCodes.Status201Created)
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();

        group.MapPut("/{clusterGuid:guid}",
                async (IClusterService clusterService, Guid clusterGuid, UpdateClusterRequest request) =>
                {
                    var cluster = await clusterService.GetByGuidAsync(clusterGuid);
                    if (cluster is null) return Results.NotFound();

                    var result = await clusterService.UpdateAsync(clusterGuid, request.Host, request.Token);
                    return result.IsSuccessful
                        ? Results.Ok(new { result.Value.Guid, result.Value.Host })
                        : Results.BadRequest(result.Error);
                })
            .WithName("UpdateCluster")
            .Produces<ClusterResponse>()
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();

        group.MapDelete("/{clusterGuid:guid}", async (IClusterService clusterService, Guid clusterGuid) =>
            {
                var cluster = await clusterService.GetByGuidAsync(clusterGuid);
                if (cluster is null) return Results.NotFound();

                var result = await clusterService.DeleteAsync(clusterGuid);
                return result.IsSuccessful ? Results.NoContent() : Results.BadRequest(result.Error);
            })
            .WithName("UnregisterCluster")
            .Produces(StatusCodes.Status204NoContent)
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();

        group.MapGet("/{clusterGuid:guid}/knowledge-bases",
                async (IClusterService clusterService, Guid clusterGuid) =>
                {
                    var cluster = await clusterService.GetByGuidAsync(clusterGuid);
                    if (cluster is null) return Results.NotFound();

                    return Results.Ok(cluster.KnowledgeBases.Select(kb => new { kb.Guid, kb.Name, kb.Description }));
                })
            .WithName("GetClusterKnowledgeBases")
            .Produces<IEnumerable<KnowledgeBaseResponse>>()
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();

        group.MapPost("/{clusterGuid:guid}/knowledge-bases/{knowledgeBaseGuid:guid}",
                async (IClusterService clusterService, IKnowledgeBaseService knowledgeBaseService, Guid clusterGuid,
                    Guid knowledgeBaseGuid) =>
                {
                    var cluster = await clusterService.GetByGuidAsync(clusterGuid);
                    if (cluster is null) return Results.NotFound();

                    var knowledgeBase = await knowledgeBaseService.GetByGuidAsync(knowledgeBaseGuid);
                    if (knowledgeBase is null) return Results.NotFound();

                    var result = await clusterService.LinkAsync(cluster, knowledgeBase);

                    return result.IsSuccessful ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
                })
            .WithName("LinkClusterToKnowledgeBase")
            .Produces(StatusCodes.Status204NoContent)
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();

        group.MapDelete("/{clusterGuid:guid}/knowledge-bases/{knowledgeBaseGuid:guid}",
                async (IClusterService clusterService, IKnowledgeBaseService knowledgeBaseService, Guid clusterGuid,
                    Guid knowledgeBaseGuid) =>
                {
                    var cluster = await clusterService.GetByGuidAsync(clusterGuid);
                    if (cluster is null) return Results.NotFound();

                    var knowledgeBase = await knowledgeBaseService.GetByGuidAsync(knowledgeBaseGuid);
                    if (knowledgeBase is null) return Results.NotFound();

                    var result = await clusterService.UnlinkAsync(cluster, knowledgeBase);

                    return result.IsSuccessful ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
                })
            .WithName("UnlinkClusterFromKnowledgeBase")
            .Produces(StatusCodes.Status204NoContent)
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();

        group.MapPost("/{clusterGuid:guid}/logs/recommend-fixes",
                async (IClusterService clusterService, Guid clusterGuid,
                    HowToFixRequest request) =>
                {
                    var cluster = await clusterService.GetByGuidAsync(clusterGuid);
                    if (cluster is null) return Results.NotFound();

                    var result = await clusterService.AnalyzeLogsAsync(cluster, request.Namespace,
                        request.Pod, request.Tail, request.SinceTime);
                    return result.IsSuccessful ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
                })
            .WithName("RecommendFixes")
            .Produces<TroubleshootingResponse>()
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();
    }
}