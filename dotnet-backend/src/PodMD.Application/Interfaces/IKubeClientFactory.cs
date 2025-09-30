using k8s;

namespace PodMD.Application.Interfaces;

public interface IKubeClientFactory
{
    Task<IKubernetes> CreateClientAsync(Guid clusterId);
}
