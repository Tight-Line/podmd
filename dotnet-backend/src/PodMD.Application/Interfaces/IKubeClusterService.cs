using PodMD.Application.Dtos;

namespace PodMD.Application.Interfaces;

public interface IKubeClusterService
{
    Task<KubeClusterResponse> CreateAsync(CreateKubeClusterRequest request);
    Task<KubeClusterResponse?> GetByIdAsync(Guid id);
    Task<IEnumerable<KubeClusterResponse>> GetAllAsync();
    Task<KubeClusterResponse> UpdateAsync(Guid id, UpdateKubeClusterRequest request);
    Task DeleteAsync(Guid id);
}
