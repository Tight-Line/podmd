using PodMD.Api.Authentication;
using PodMD.Api.Requests;
using PodMD.Api.Responses;
using PodMD.Api.Services;

namespace PodMD.Api.Endpoints;

public static class RagEndpoints
{
    public static void MapRagEndpoints(this WebApplication app)
    {
        var clusterRagGroup = app.MapGroup("/clusters/{clusterGuid:guid}/rag").WithTags("Clusters");

        var ragGroup = app.MapGroup("/rag").WithTags("RAG");

        clusterRagGroup.MapPost("/enable",
                async (IRagService ragService, IClusterService clusterService, Guid clusterGuid) =>
                {
                    var cluster = await clusterService.GetByGuidAsync(clusterGuid);
                    if (cluster is null) return Results.NotFound();

                    if (cluster.OpenAIAssistantId is not null)
                        return Results.Conflict("RAG is already enabled for this cluster.");

                    var result = await ragService.CreateOpenAIAssistantAsync(cluster);
                    return result.IsSuccessful
                        ? Results.Ok(new { result.Value.Guid, result.Value.Host })
                        : Results.BadRequest(result.Error);
                })
            .WithName("Enable")
            .Produces(StatusCodes.Status200OK)
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();

        clusterRagGroup.MapPost("/enable-and-upload",
                async (IRagService ragService, IClusterService clusterService,
                    Guid clusterGuid,
                    IFormFileCollection files) =>
                {
                    var cluster = await clusterService.GetByGuidAsync(clusterGuid);
                    if (cluster is null) return Results.NotFound();

                    if (cluster.OpenAIAssistantId is not null)
                        return Results.Conflict("RAG is already enabled for this cluster.");

                    var result = await ragService.CreateOpenAIAssistantAsync(cluster, files);
                    return result.IsSuccessful
                        ? Results.Ok(result.Value.Select(r => new { r.Guid, r.Name }))
                        : Results.BadRequest(result.Error);
                })
            .WithName("EnableAndUpload")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<List<RagResourceResponse>>()
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>()
            .DisableAntiforgery();

        ragGroup.MapPost("/resources/upload", async (IRagService ragService, IFormFile file) =>
            {
                if (file.Length == 0)
                    return Results.BadRequest("No file uploaded.");

                await using var stream = file.OpenReadStream();
                var result = await ragService.UploadAsync(file.FileName, stream);

                return result.IsSuccessful
                    ? Results.Ok(new { result.Value.Guid, result.Value.Name })
                    : Results.BadRequest(result.Error);
            })
            .WithName("Upload")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<RagResourceResponse>()
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>()
            .DisableAntiforgery();

        ragGroup.MapPost("/link",
                async (IClusterService clusterService, IRagService ragService, LinkRagResourceRequest request) =>
                {
                    var cluster = await clusterService.GetByGuidAsync(request.clusterGuid);
                    if (cluster is null) return Results.NotFound();

                    var ragResource = await ragService.GetByGuid(request.ragResourceGuid);
                    if (ragResource is null) return Results.NotFound();

                    var result = await ragService.LinkAsync(cluster, ragResource);

                    return result.IsSuccessful
                        ? Results.Ok()
                        : Results.BadRequest(result.Error);
                })
            .WithName("Link")
            .Produces(StatusCodes.Status200OK)
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();
    }
}