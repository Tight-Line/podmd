using PodMD.Api.Authentication;
using PodMD.Api.Responses;
using PodMD.Api.Services;

namespace PodMD.Api.Endpoints;

public static class ConfigurationEndpoints
{
    public static void MapConfigurationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/configurations").WithTags("Configurations");

        group.MapGet("", async (IConfigurationService configurationService) =>
            {
                var result = await configurationService.GetAllAsync();
                return result.IsSuccessful
                    ? Results.Ok(result.Value.Select(c => new
                        { c.Guid, c.Name, c.Host, c.CreatedAt, Type = c.GetType().Name }))
                    : Results.BadRequest(result.Error);
            })
            .WithName("GetConfigurations")
            .Produces<IEnumerable<ConfigurationResponse>>()
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();

        group.MapDelete("/{configGuid:guid}", async (IConfigurationService configurationService, Guid configGuid) =>
            {
                var config = await configurationService.GetByGuidAsync(configGuid);
                if (config is null) return Results.NotFound();

                var result = await configurationService.DeleteAsync(configGuid);
                return result.IsSuccessful ? Results.NoContent() : Results.BadRequest(result.Error);
            })
            .WithName("RemoveConfiguration")
            .Produces(StatusCodes.Status204NoContent)
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();

        group.MapGet("/{configGuid:guid}/knowledge-bases",
                async (IConfigurationService configurationService, Guid configGuid) =>
                {
                    var config = await configurationService.GetByGuidAsync(configGuid);
                    if (config is null) return Results.NotFound();

                    return Results.Ok(config.KnowledgeBases.Select(kb =>
                        new { kb.Guid, kb.Name, kb.Description, kb.CreatedAt }));
                })
            .WithName("GetConfigurationsKnowledgeBases")
            .Produces<IEnumerable<KnowledgeBaseResponse>>()
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();

        group.MapGet("/{configGuid:guid}/available-knowledge-bases",
                async (IConfigurationService configurationService, Guid configGuid) =>
                {
                    var config = await configurationService.GetByGuidAsync(configGuid);
                    if (config is null) return Results.NotFound();

                    var result = await configurationService.GetAvailableKnowledgeBases(configGuid);

                    return result.IsSuccessful
                        ? Results.Ok(result.Value.Select(kb => new { kb.Guid, kb.Name, kb.Description, kb.CreatedAt }))
                        : Results.BadRequest(result.Error);
                })
            .WithName("GetAvailableKnowledgeBases")
            .Produces<IEnumerable<KnowledgeBaseResponse>>()
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();

        group.MapPost("/{configGuid:guid}/knowledge-bases/{kbGuid:guid}",
                async (IConfigurationService configurationService, IKnowledgeBaseService knowledgeBaseService,
                    Guid configGuid,
                    Guid kbGuid) =>
                {
                    var config = await configurationService.GetByGuidAsync(configGuid);
                    if (config is null) return Results.NotFound();

                    var kb = await knowledgeBaseService.GetByGuidAsync(kbGuid);
                    if (kb is null) return Results.NotFound();

                    var result = await configurationService.LinkAsync(config, kb);

                    return result.IsSuccessful ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
                })
            .WithName("LinkConfigurationToKnowledgeBase")
            .Produces(StatusCodes.Status204NoContent)
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();

        group.MapDelete("/{configGuid:guid}/knowledge-bases/{kbGuid:guid}",
                async (IConfigurationService configurationService, IKnowledgeBaseService knowledgeBaseService,
                    Guid configGuid,
                    Guid kbGuid) =>
                {
                    var config = await configurationService.GetByGuidAsync(configGuid);
                    if (config is null) return Results.NotFound();

                    var kb = await knowledgeBaseService.GetByGuidAsync(kbGuid);
                    if (kb is null) return Results.NotFound();

                    var result = await configurationService.UnlinkAsync(config, kb);

                    return result.IsSuccessful ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
                })
            .WithName("UnlinkConfigurationFromKnowledgeBase")
            .Produces(StatusCodes.Status204NoContent)
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();
    }
}