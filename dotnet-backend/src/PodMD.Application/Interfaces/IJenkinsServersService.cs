using PodMD.Application.Dtos;

namespace PodMD.Application.Interfaces;

public interface IJenkinsServersService
{
    Task<JenkinsServersResponse> CreateAsync(CreateJenkinsServersRequest request);
    Task<JenkinsServersResponse?> GetByIdAsync(Guid id);
    Task<IEnumerable<JenkinsServersResponse>> GetAllAsync();
    Task<JenkinsServersResponse> UpdateAsync(Guid id, UpdateJenkinsServersRequest request);
    Task DeleteAsync(Guid id);
}
