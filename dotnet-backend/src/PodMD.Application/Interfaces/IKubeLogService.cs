using PodMD.Application.Dtos;

namespace PodMD.Application.Interfaces;

public interface IKubeLogService
{
    Task<LogResult> GetPodLogsAsync(Guid clusterId, PodLogParameters parameters);
    Task<LogResult> GetDeploymentLogsAsync(Guid clusterId, DeploymentLogParameters parameters);
}
