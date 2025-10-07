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
    private readonly ILogger<AnalysisService> _logger;

    public AnalysisService(
        IKubeLogService logService,
        ILlmClient llmClient,
        IKubeClusterRepository clusterRepository,
        ILogger<AnalysisService> logger)
    {
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        _llmClient = llmClient ?? throw new ArgumentNullException(nameof(llmClient));
        _clusterRepository = clusterRepository ?? throw new ArgumentNullException(nameof(clusterRepository));
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
        bool? includeDescription = null,
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
                limitBytes,
                includeDescription
            );

            var logResult = await _logService.GetPodLogsAsync(clusterId, parameters);

            if (string.IsNullOrWhiteSpace(logResult.Logs))
            {
                return new AnalysisResponse
                {
                    Success = false,
                    Message = "No logs found for analysis"
                };
            }

            // Analyze logs
            var analysisResponse = await AnalyzeLogsAsync(clusterId, logResult.Logs, cancellationToken);
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
        bool? includeDescription = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get logs using existing service
            var parameters = new DeploymentLogParameters(
                namespaceName,
                deploymentName,
                fallback,
                includeDescription
            );

            var logResult = await _logService.GetDeploymentLogsAsync(clusterId, parameters);

            if (string.IsNullOrWhiteSpace(logResult.Logs))
            {
                return new AnalysisResponse
                {
                    Success = false,
                    Message = "No logs found for analysis"
                };
            }

            // Analyze logs
            var analysisResponse = await AnalyzeLogsAsync(clusterId, logResult.Logs, cancellationToken);
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

    private async Task<AnalysisResponse> AnalyzeLogsAsync(Guid clusterId, string logs, CancellationToken cancellationToken)
    {
        // Get cluster configuration
        var cluster = await _clusterRepository.GetByIdAsync(clusterId);

        // Determine if custom response format is being used
        var isDefaultFormat = string.IsNullOrWhiteSpace(cluster?.ResponseFormat) ||
                              cluster.ResponseFormat == AnalysisDefaults.DefaultResponseFormat;

        // Build comprehensive instructions
        var baseInstructions = cluster?.Instructions ?? AnalysisDefaults.TroubleshootingPrompt;
        var jsonSchema = cluster?.ResponseFormat ?? AnalysisDefaults.DefaultResponseFormat;

        // Log format usage for monitoring
        if (!isDefaultFormat)
        {
            _logger.LogInformation("Cluster {ClusterId} using custom response format", clusterId);
        }

        // Combine instructions with format and guidelines
        var instructions = $@"{baseInstructions}

Return a JSON object in this format: {jsonSchema}";

        // Call LLM
        var llmResponse = await _llmClient.AnalyzeLogsAsync(logs, instructions, cancellationToken);

        // Clean LLM response (remove markdown code blocks, etc.)
        var cleanResponse = CleanLlmResponse(llmResponse);

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
