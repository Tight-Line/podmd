using System.Security.Claims;

namespace PodMD.Api.Services;

public interface IAuthenticatedApiKeyService
{
    uint ApiKeyId { get; }
    bool IsAuthenticated { get; }
}

public class AuthenticatedApiKeyService(IHttpContextAccessor httpContextAccessor) : IAuthenticatedApiKeyService
{
    public uint ApiKeyId =>
        uint.TryParse(httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : 0;

    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}