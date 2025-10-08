using PodMD.Application.Dtos;

namespace PodMD.Application.Interfaces;

public interface IApiKeyService
{
    Task<CreateApiKeyResponse> CreateApiKeyAsync(string userId, CreateApiKeyRequest request);
    Task<ApiKeyDto?> GetByIdAsync(Guid id, string userId);
    Task<IEnumerable<ApiKeyDto>> GetAllByUserIdAsync(string userId);
    Task<ApiKeyDto?> UpdateApiKeyAsync(Guid id, string userId, UpdateApiKeyRequest request);
    Task DeleteApiKeyAsync(Guid id, string userId);
}
