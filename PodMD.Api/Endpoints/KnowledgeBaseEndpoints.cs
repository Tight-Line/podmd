using PodMD.Api.Authentication;
using PodMD.Api.Requests;
using PodMD.Api.Responses;
using PodMD.Api.Services;

namespace PodMD.Api.Endpoints;

public static class KnowledgeBaseEndpoints
{
    public static void MapKnowledgeBaseEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/knowledge-bases").WithTags("Knowledge Bases");

        group.MapGet("", async (IKnowledgeBaseService knowledgeBaseService) =>
            {
                var result = await knowledgeBaseService.GetAllAsync();
                return result.IsSuccessful
                    ? Results.Ok(result.Value.Select(kb => new { kb.Guid, kb.Name, kb.Description }))
                    : Results.BadRequest(result.Error);
            })
            .WithName("GetKnowledgeBases")
            .Produces<IEnumerable<KnowledgeBaseResponse>>()
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();

        group.MapPost("", async (IKnowledgeBaseService knowledgeBaseService, CreateKnowledgeBaseRequest request) =>
            {
                var result = await knowledgeBaseService.CreateAsync(request.Name, request.Description);
                return result.IsSuccessful
                    ? Results.Ok(new { result.Value.Guid, result.Value.Name, result.Value.Description })
                    : Results.BadRequest(result.Error);
            })
            .WithName("CreateKnowledgeBase")
            .Produces<KnowledgeBaseResponse>(StatusCodes.Status201Created)
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();

        group.MapGet("/{knowledgeBaseGuid:guid}",
                async (IKnowledgeBaseService knowledgeBaseService, Guid knowledgeBaseGuid) =>
                {
                    var knowledgeBase = await knowledgeBaseService.GetByGuidAsync(knowledgeBaseGuid);
                    if (knowledgeBase is null) return Results.NotFound();

                    return Results.Ok(new { knowledgeBase.Guid, knowledgeBase.Name, knowledgeBase.Description });
                })
            .WithName("GetKnowledgeBase")
            .Produces<KnowledgeBaseResponse>()
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();

        group.MapDelete("/{knowledgeBaseGuid:guid}",
                async (IKnowledgeBaseService knowledgeBaseService, Guid knowledgeBaseGuid) =>
                {
                    var knowledgeBase = await knowledgeBaseService.GetByGuidAsync(knowledgeBaseGuid);
                    if (knowledgeBase is null) return Results.NotFound();

                    var result = await knowledgeBaseService.DeleteAsync(knowledgeBase);
                    return result.IsSuccessful ? Results.NoContent() : Results.BadRequest(result.Error);
                })
            .WithName("DeleteKnowledgeBase")
            .Produces(StatusCodes.Status204NoContent)
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();

        group.MapGet("/{knowledgeBaseGuid:guid}/resources",
                async (IKnowledgeBaseService knowledgeBaseService, Guid knowledgeBaseGuid) =>
                {
                    var knowledgeBase = await knowledgeBaseService.GetByGuidAsync(knowledgeBaseGuid);
                    if (knowledgeBase is null) return Results.NotFound();

                    return Results.Ok(knowledgeBase.Resources.Select(r => new { r.Guid, r.FileName }));
                })
            .WithName("GetKnowledgeBaseResources")
            .Produces<IEnumerable<ResourceResponse>>()
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();

        group.MapPost("/{knowledgeBaseGuid:guid}/resources",
                async (IKnowledgeBaseService knowledgeBaseService, IResourceService resourceService, IFormFile file,
                    Guid knowledgeBaseGuid) =>
                {
                    var knowledgeBase = await knowledgeBaseService.GetByGuidAsync(knowledgeBaseGuid);
                    if (knowledgeBase == null) return Results.NotFound();

                    await using var stream = file.OpenReadStream();
                    var result = await resourceService.UploadAsync(knowledgeBase, file.FileName, stream);

                    return result.IsSuccessful
                        ? Results.Ok(new { result.Value.Guid, Name = result.Value.FileName })
                        : Results.BadRequest(result.Error);
                })
            .WithName("UploadResource")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<ResourceResponse>()
            .DisableAntiforgery()
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();

        group.MapPost("/{knowledgeBaseGuid:guid}/resources/{resourceGuid:guid}",
                async (IKnowledgeBaseService knowledgeBaseService, IResourceService resourceService, IFormFile file,
                    Guid knowledgeBaseGuid, Guid resourceGuid) =>
                {
                    var knowledgeBase = await knowledgeBaseService.GetByGuidAsync(knowledgeBaseGuid);
                    if (knowledgeBase == null) return Results.NotFound();

                    var resource = await resourceService.GetByGuidAsync(resourceGuid);
                    if (resource is null) return Results.NotFound();

                    await using var stream = file.OpenReadStream();
                    var result =
                        await resourceService.ReplaceAsync(knowledgeBase, resource, file.FileName, stream);

                    return result.IsSuccessful ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
                })
            .WithName("ReplaceResource")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<ResourceResponse>()
            .DisableAntiforgery()
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();

        group.MapDelete("/{knowledgeBaseGuid:guid}/resources/{resourceGuid:guid}",
                async (IKnowledgeBaseService knowledgeBaseService, IResourceService resourceService,
                    Guid knowledgeBaseGuid, Guid resourceGuid) =>
                {
                    var knowledgeBase = await knowledgeBaseService.GetByGuidAsync(knowledgeBaseGuid);
                    if (knowledgeBase == null) return Results.NotFound();

                    var resource = await resourceService.GetByGuidAsync(resourceGuid);
                    if (resource is null) return Results.NotFound();

                    var result = await resourceService.DeleteAsync(resource);

                    return result.IsSuccessful ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
                })
            .WithName("DeleteResource")
            .Produces<bool>()
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();
    }
}