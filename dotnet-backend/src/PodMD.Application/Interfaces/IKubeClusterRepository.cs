using PodMD.Domain.Entities;

namespace PodMD.Application.Interfaces;

public interface IKubeClusterRepository
{
    Task<KubeCluster?> GetByIdAsync(Guid id);
    Task<KubeCluster?> GetByNameAsync(string name);
    Task<IEnumerable<KubeCluster>> GetAllAsync();
    Task<KubeCluster> AddAsync(KubeCluster cluster);
    Task UpdateAsync(KubeCluster cluster);
    Task DeleteAsync(KubeCluster cluster);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null);
}
