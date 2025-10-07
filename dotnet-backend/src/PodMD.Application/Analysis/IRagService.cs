namespace PodMD.Application.Analysis;

public interface IRagService
{
    /// <summary>
    /// Retrieves all available knowledge context for LLM analysis
    /// </summary>
    /// <param name="clusterId">The cluster ID to find connected knowledge bases</param>
    /// <param name="maxContextTokens">Maximum tokens to include in context (default 1024)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Analysis context with retrieved knowledge</returns>
    Task<AnalysisRagContext> GetRelevantContextAsync(
        Guid clusterId,
        int maxContextTokens = 1024,
        CancellationToken cancellationToken = default);
}

public class AnalysisRagContext
{
    public bool IsRagEnabled { get; set; }
    public string RetrievedKnowledge { get; set; } = string.Empty;
    public int KnowledgeTokens { get; set; }
    public int SourcesUsed { get; set; }
    public int ChunksRetrieved { get; set; }
    public string RagPerformanceMetrics { get; set; } = string.Empty;
}
