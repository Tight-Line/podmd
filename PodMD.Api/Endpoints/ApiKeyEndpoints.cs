using PodMD.Api.Responses;
using PodMD.Api.Services;

namespace PodMD.Api.Endpoints;

public static class ApiKeyEndpoints
{
    public static void MapApiKeyEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api-keys").WithTags("API Keys");

        group.MapPost("/", async (IApiKeyService apiKeyService) =>
            {
                var result = await apiKeyService.CreateAsync();
                return result.IsSuccessful
                    ? Results.Ok(new
                    {
                        ApiKey = result.Value.PlainTextKey,
                        result.Value.ApiKey.LookupName,
                        result.Value.ApiKey.CreatedAt
                    })
                    : Results.BadRequest(result.Error);
            })
            .WithName("GetApiKey")
            .Produces<ApiKeyResponse>();
    }
}