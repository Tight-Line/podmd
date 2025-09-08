using PodMD.Api.Authentication;
using PodMD.Api.Requests;
using PodMD.Api.Responses;
using PodMD.Api.Services;

namespace PodMD.Api.Endpoints;

public static class JenkinsServerEndpoints
{
    public static void MapJenkinsServerEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/configurations/jenkins-servers").WithTags("Jenkins Server Configurations");

        group.MapGet("", async (IJenkinsServerService jenkinsServerService) =>
            {
                var result = await jenkinsServerService.GetAllAsync();
                return result.IsSuccessful
                    ? Results.Ok(result.Value.Select(js => new { js.Guid, js.Name, Url = js.Host }))
                    : Results.BadRequest(result.Error);
            })
            .WithName("GetJenkinsServers")
            .Produces<IEnumerable<JenkinsServerResponse>>()
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();

        group.MapPost("", async (IJenkinsServerService jenkinsServerService, CreateJenkinsServerRequest request) =>
            {
                var result = await jenkinsServerService.CreateAsync(request.Name, request.Host, request.Username,
                    request.ApiToken);
                return result.IsSuccessful
                    ? Results.Ok(new { result.Value.Guid, result.Value.Name, result.Value.Host })
                    : Results.BadRequest(result.Error);
            })
            .WithName("RegisterJenkinsServer")
            .Produces<JenkinsServerResponse>(StatusCodes.Status201Created)
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();

        group.MapPut("/{configGuid:guid}",
                async (IJenkinsServerService jenkinsServerService, Guid configGuid,
                    UpdateJenkinsServerRequest request) =>
                {
                    var jenkinsServer = await jenkinsServerService.GetByGuidAsync(configGuid);
                    if (jenkinsServer is null) return Results.NotFound();

                    var result = await jenkinsServerService.UpdateAsync(jenkinsServer, request.Name, request.Host,
                        request.Username,
                        request.Token);
                    return result.IsSuccessful
                        ? Results.Ok(new { result.Value.Guid, result.Value.Name, result.Value.Host })
                        : Results.BadRequest(result.Error);
                })
            .WithName("UpdateJenkinsServer")
            .Produces<JenkinsServerResponse>()
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();

        group.MapDelete("/{configGuid:guid}", async (IJenkinsServerService jenkinsServerService, Guid configGuid) =>
            {
                var cluster = await jenkinsServerService.GetByGuidAsync(configGuid);
                if (cluster is null) return Results.NotFound();

                var result = await jenkinsServerService.DeleteAsync(configGuid);
                return result.IsSuccessful ? Results.NoContent() : Results.BadRequest(result.Error);
            })
            .WithName("UnregisterJenkinsServer")
            .Produces(StatusCodes.Status204NoContent)
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();

        group.MapPost("/test-connection",
                async (IJenkinsServerService jenkinsServerService, CreateJenkinsServerRequest request) =>
                {
                    var result =
                        await jenkinsServerService.TestConnectionAsync(request.Host, request.Username, request.ApiToken);
                    return result.IsSuccessful
                        ? Results.Ok(result.Value)
                        : Results.BadRequest(result.Error);
                })
            .WithName("TestJenkinsServerConnection")
            .Produces<bool>()
            .AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>();
    }
}