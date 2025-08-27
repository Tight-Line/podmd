using System.Security.Claims;
using PodMD.Api.Services;

namespace PodMD.Api.Authentication;

public class ApiKeyAuthenticationEndpointFilter(IHashingService hashingService, IApiKeyService apiKeyService)
    : IEndpointFilter
{
    private const string ApiKeyHeaderName = "X-Api-Key";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;

        string? apiKey = context.HttpContext.Request.Headers[ApiKeyHeaderName];
        if (string.IsNullOrWhiteSpace(apiKey)) return Results.Unauthorized();

        var parts = apiKey.Split('-');
        var lookupName = parts[0];
        var secret = parts[1];

        var apiKeyEntity = await apiKeyService.GetByLookupNameAsync(lookupName);
        if (apiKeyEntity is null) return Results.Unauthorized();

        var secretHash = hashingService.Hash(secret);

        if (!hashingService.Verify(secretHash, apiKeyEntity.SecretHash)) return Results.Unauthorized();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, apiKeyEntity.Id.ToString())
        };

        var identity = new ClaimsIdentity(claims, "ApiKey");
        var principal = new ClaimsPrincipal(identity);
        httpContext.User = principal;

        return await next(context);
    }
}