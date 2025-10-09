using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using PodMD.Application.Dtos;
using PodMD.Application.Interfaces;

namespace PodMD.Application.Analysis;

public class LogError
{
    public string Description { get; set; } = string.Empty;
    public List<string> Occurrences { get; set; } = new();
    public List<Solution> Solutions { get; set; } = new();
}

public class Solution
{
    public string Description { get; set; } = string.Empty;
    public List<Step> Steps { get; set; } = new();
}

public class Step
{
    public string Title { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
}

public class AnalysisResult
{
    public List<LogError> Errors { get; set; } = new();
}

public class AnalysisService : IAnalysisService
{
    private readonly IKubeLogService _logService;
    private readonly ILlmClient _llmClient;
    private readonly IKubeClusterRepository _clusterRepository;
    private readonly IRagService _ragService;
    private readonly ILogger<AnalysisService> _logger;

    public AnalysisService(
        IKubeLogService logService,
        ILlmClient llmClient,
        IKubeClusterRepository clusterRepository,
        IRagService ragService,
        ILogger<AnalysisService> logger)
    {
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        _llmClient = llmClient ?? throw new ArgumentNullException(nameof(llmClient));
        _clusterRepository = clusterRepository ?? throw new ArgumentNullException(nameof(clusterRepository));
        _ragService = ragService ?? throw new ArgumentNullException(nameof(ragService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AnalysisResponse> AnalyzePodLogsAsync(
        Guid clusterId,
        string namespaceName,
        string podName,
        string? containerName = null,
        int? tailLines = null,
        int? sinceSeconds = null,
        bool? previous = null,
        int? limitBytes = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get logs using existing service
            var parameters = new PodLogParameters(
                namespaceName,
                podName,
                containerName,
                tailLines,
                sinceSeconds,
                previous,
                limitBytes
            );

            var logResult = await _logService.GetPodLogsAsync(clusterId, parameters);

            if (string.IsNullOrWhiteSpace(logResult.Logs))
            {
                _logger.LogWarning("No logs found for pod {PodName} in namespace {Namespace}, proceeding with description-only analysis", parameters.PodName, parameters.Namespace);
            }

            // Analyze logs with description context (proceed even with empty logs)
            var analysisResponse = await AnalyzeLogsAsync(clusterId, logResult.Logs ?? "", logResult.Description, cancellationToken);
            return analysisResponse;
        }
        catch (Exception ex)
        {
            return new AnalysisResponse
            {
                Success = false,
                Message = $"Analysis failed: {ex.Message}"
            };
        }
    }

    public async Task<AnalysisResponse> AnalyzeDeploymentLogsAsync(
        Guid clusterId,
        string namespaceName,
        string deploymentName,
        bool? fallback = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get logs using existing service
            var parameters = new DeploymentLogParameters(
                namespaceName,
                deploymentName,
                fallback
            );

            var logResult = await _logService.GetDeploymentLogsAsync(clusterId, parameters);

            if (string.IsNullOrWhiteSpace(logResult.Logs))
            {
                _logger.LogWarning("No logs found for deployment {DeploymentName} in namespace {Namespace}, proceeding with description-only analysis", parameters.DeploymentName, parameters.Namespace);
            }

            // Analyze logs with description context (proceed even with empty logs)
            var analysisResponse = await AnalyzeLogsAsync(clusterId, logResult.Logs ?? "", logResult.Description, cancellationToken);
            return analysisResponse;
        }
        catch (Exception ex)
        {
            return new AnalysisResponse
            {
                Success = false,
                Message = $"Analysis failed: {ex.Message}"
            };
        }
    }

    private async Task<AnalysisResponse> AnalyzeLogsAsync(Guid clusterId, string logs, string? description, CancellationToken cancellationToken)
    {
        // Get cluster configuration
        var cluster = await _clusterRepository.GetByIdAsync(clusterId);

        // Determine if custom response format is being used
        var isDefaultFormat = string.IsNullOrWhiteSpace(cluster?.ResponseFormat) ||
                              cluster.ResponseFormat == AnalysisDefaults.DefaultResponseFormat;

        // Try to get RAG context (gracefully fails if not available)
        var ragContext = await GetRagContextSafeAsync(clusterId, logs, cancellationToken);

        // Build comprehensive instructions
        var baseInstructions = cluster?.Instructions ?? AnalysisDefaults.TroubleshootingPrompt;
        var jsonSchema = cluster?.ResponseFormat ?? AnalysisDefaults.DefaultResponseFormat;

        var instructionsBuilder = new System.Text.StringBuilder();

        // Add RAG context if available
        if (ragContext.IsRagEnabled && !string.IsNullOrWhiteSpace(ragContext.RetrievedKnowledge))
        {
            _logger.LogInformation("📚 RAG ACTIVE: Analyzing with enhanced context for cluster {ClusterId} - {Tokens} tokens from {Sources} knowledge sources",
                clusterId, ragContext.KnowledgeTokens, ragContext.SourcesUsed);

            instructionsBuilder.AppendLine("Relevant Knowledge Context:");
            instructionsBuilder.AppendLine(ragContext.RetrievedKnowledge);
            instructionsBuilder.AppendLine();
        }
        else
        {
            _logger.LogDebug("Base LLM analysis: RAG not available for cluster {ClusterId}", clusterId);
        }

        instructionsBuilder.AppendLine(baseInstructions);

        // Combine instructions with format and guidelines
        var instructions = $@"{instructionsBuilder.ToString().Trim()}

Return a JSON object in this format: {jsonSchema}";

        // Log format usage for monitoring
        if (!isDefaultFormat)
        {
            _logger.LogInformation("Cluster {ClusterId} using custom response format", clusterId);
        }

        _logger.LogInformation("Instructions: {instr}", instructions);

        // Call LLM
        var llmResponse = await _llmClient.AnalyzeLogsAsync(logs, instructions, description, cancellationToken);

        // Clean LLM response (remove markdown code blocks, etc.)
        var cleanResponse = CleanLlmResponse(llmResponse);

        // Log the cleaned LLM response for debugging
        _logger.LogInformation("LLM response for cluster {ClusterId}: {CleanResponse}", clusterId, cleanResponse);

        if (string.IsNullOrWhiteSpace(cleanResponse))
        {
            _logger.LogError("LLM returned empty or invalid response for cluster {ClusterId}", clusterId);
            return new AnalysisResponse
            {
                Success = false,
                Message = "LLM returned invalid or empty response",
                ResultFormat = isDefaultFormat ? ResultFormat.Default : ResultFormat.Custom
            };
        }

        // Deserialize based on expected format
        if (isDefaultFormat)
        {
            // Try to deserialize as standard AnalysisResult
            try
            {
                var result = JsonSerializer.Deserialize<AnalysisResult>(cleanResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Validate structure (basic validation)
                if (result == null || result.Errors == null)
                {
                    throw new JsonException("Invalid response structure - missing required fields");
                }

                _logger.LogDebug("Successfully parsed response as AnalysisResult for cluster {ClusterId}", clusterId);
                return new AnalysisResponse
                {
                    Success = true,
                    Data = result,
                    Message = "Analysis completed successfully",
                    ResultFormat = ResultFormat.Default
                };
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse LLM response as AnalysisResult for cluster {ClusterId} using default format: {CleanResponse}", clusterId, cleanResponse);

                return new AnalysisResponse
                {
                    Success = false,
                    Message = "Failed to parse LLM response - invalid JSON structure for default format",
                    ResultFormat = ResultFormat.Default
                };
            }
        }
        else
        {
            // Parse as raw JSON document for custom formats
            try
            {
                var jsonDocument = JsonDocument.Parse(cleanResponse);

                _logger.LogDebug("Successfully parsed response as custom JSON format for cluster {ClusterId}", clusterId);
                return new AnalysisResponse
                {
                    Success = true,
                    Data = jsonDocument.RootElement,
                    Message = "Analysis completed with custom format",
                    ResultFormat = ResultFormat.Custom
                };
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse LLM response as JSON for cluster {ClusterId} using custom format: {CleanResponse}", clusterId, cleanResponse);

                return new AnalysisResponse
                {
                    Success = false,
                    Message = "Failed to parse LLM response - invalid JSON format",
                    ResultFormat = ResultFormat.Custom
                };
            }
        }
    }

    private async Task<AnalysisRagContext> GetRagContextSafeAsync(Guid clusterId, string query, CancellationToken cancellationToken)
    {
        try
        {
            // Extract a meaningful query from logs for semantic search
            // Use first 500 characters as representative sample, or first few lines
            var representativeQuery = ExtractRepresentativeQuery(query, 500);
            var ragContext = await _ragService.GetRelevantContextAsync(clusterId, 1024, cancellationToken);
            return ragContext;
        }
        catch (Exception ex)
        {
            // RAG failures should not break analysis - log and continue without RAG
            _logger.LogWarning(ex, "RAG context retrieval failed for cluster {ClusterId}, continuing without RAG: {Message}",
                clusterId, ex.Message);
            return new AnalysisRagContext(); // Return empty context
        }
    }

    private static string ExtractRepresentativeQuery(string logs, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(logs))
        {
            return "error logs analysis";
        }

        // Take first few lines to get representative sample
        var lines = logs.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var representativeLines = lines.Take(10); // First 10 lines should give good context
        var repQuery = string.Join(" ", representativeLines);

        // Truncate if too long
        if (repQuery.Length > maxLength)
        {
            repQuery = repQuery.Substring(0, maxLength).TrimEnd();
        }

        return repQuery.Length > 10 ? repQuery : "kubernetes error logs analysis";
    }

    private string CleanLlmResponse(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
            return response;

        // Try to extract JSON from markdown code blocks
        var jsonBlockPattern = @"```(?:json)?\s*(\{[\s\S]*?\})\s*```";
        var match = Regex.Match(response, jsonBlockPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        // Fallback: look for the first { and last } to extract JSON object
        var startIndex = response.IndexOf('{');
        var lastIndex = response.LastIndexOf('}');

        if (startIndex >= 0 && lastIndex > startIndex)
        {
            var extracted = response.Substring(startIndex, lastIndex - startIndex + 1).Trim();
            // Remove any leading/trailing quotes or backticks that might still be there
            extracted = extracted.Trim('"', '`', '\'', '\n', '\r', '\t', ' ');
            return extracted;
        }

        // If no JSON found, return original response (let JSON parser fail with meaningful message)
        return response.Trim();
    }
}
