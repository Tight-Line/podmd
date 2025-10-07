using PodMD.Application.Dtos;

namespace PodMD.Application.Analysis;

public enum ResultFormat
{
    Default,  // Standard AnalysisResult structure
    Custom    // Raw JSON structure from LLM
}

public interface IAnalysisService
{
    Task<AnalysisResponse> AnalyzePodLogsAsync(Guid clusterId, string namespaceName, string podName, string? containerName = null, int? tailLines = null, int? sinceSeconds = null, bool? previous = null, int? limitBytes = null, bool? includeDescription = null, CancellationToken cancellationToken = default);
    Task<AnalysisResponse> AnalyzeDeploymentLogsAsync(Guid clusterId, string namespaceName, string deploymentName, bool? fallback = null, bool? includeDescription = null, CancellationToken cancellationToken = default);
}

public class AnalysisResponse
{
    public bool Success { get; set; }
    public object? Data { get; set; }
    public string? Message { get; set; }
    public ResultFormat ResultFormat { get; set; } = ResultFormat.Default;
}
