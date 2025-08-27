namespace PodMD.Api.Services;

public interface IAuthenticatedApiKeyService
{
    uint ApiKeyId { get; }
    bool IsAuthenticated { get; }
}

public class AuthenticatedApiKeyService(uint? apiKeyId) : IAuthenticatedApiKeyService
{
    public uint ApiKeyId => (uint)apiKeyId!;
    public bool IsAuthenticated => apiKeyId.HasValue;
}