using PodMD.Domain.Entities;

namespace PodMD.Application.Interfaces;

public interface IApiKeyRepository
{
    Task<ApiKey?> GetByIdAsync(Guid id);
    Task<ApiKey?> GetByHashedKeyAsync(string hashedKey);
    Task<IEnumerable<ApiKey>> GetByUserIdAsync(string userId);
    Task<ApiKey> AddAsync(ApiKey apiKey);
    Task UpdateAsync(ApiKey apiKey);
    Task DeleteAsync(ApiKey apiKey);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> ExistsAsync(string hashedKey);
}
