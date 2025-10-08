using Microsoft.AspNetCore.Http;
using PodMD.Application.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PodMD.Api.Middleware;

public class ApiKeyAuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public ApiKeyAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IApiKeyAuthenticationService apiKeyAuthService)
    {
        var path = context.Request.Path.ToString().ToLowerInvariant();

        // Skip authentication endpoints (login, register, validate-key)
        if (path.StartsWith("/api/v1/auth") &&
            (path.Contains("/login") || path.Contains("/register") || path.Contains("/validate-key")))
        {
            await _next(context);
            return;
        }

        // Check if the request has an X-API-Key header
        if (context.Request.Headers.TryGetValue("X-API-Key", out var apiKeyValues))
        {
            var apiKey = apiKeyValues.ToString();
            if (!string.IsNullOrEmpty(apiKey))
            {
                var userDto = await apiKeyAuthService.AuthenticateApiKeyAsync(apiKey);
                if (userDto != null)
                {
                    // Create claims principal for API key authentication
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, userDto.Id),
                        new Claim(ClaimTypes.Name, userDto.FirstName + " " + userDto.LastName),
                        new Claim(ClaimTypes.Email, userDto.Email),
                        new Claim("api_key", "true") // Mark this as API key authentication
                    };

                    var identity = new ClaimsIdentity(claims, "API Key");
                    context.User = new ClaimsPrincipal(identity);
                }
            }
        }

        await _next(context);
    }
}
