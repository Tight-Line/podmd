using PodMD.Application.Dtos;

namespace PodMD.Application.Interfaces;

public interface IApiKeyAuthenticationService
{
    Task<UserDto?> AuthenticateApiKeyAsync(string apiKey);
    Task<bool> IsUsageLimitExceededAsync(string hashedKey);
    Task IncrementUsageAsync(string hashedKey);
}
