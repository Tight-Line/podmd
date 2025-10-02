using PodMD.Domain.Entities;

namespace PodMD.Application.Interfaces;

public interface IJenkinsServersRepository
{
    Task<JenkinsServers?> GetByIdAsync(Guid id);
    Task<IEnumerable<JenkinsServers>> GetAllAsync();
    Task<JenkinsServers> AddAsync(JenkinsServers server);
    Task UpdateAsync(JenkinsServers server);
    Task DeleteAsync(JenkinsServers server);
    Task<bool> ExistsAsync(Guid id);
}
