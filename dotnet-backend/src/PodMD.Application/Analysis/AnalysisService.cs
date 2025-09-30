using System.Text.Json;
using System.Text.RegularExpressions;
using PodMD.Application.Dtos;
using PodMD.Application.Interfaces;

namespace PodMD.Application.Analysis;

public class LogError
{
    public string GeneralMessage { get; set; } = string.Empty;
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

    public AnalysisService(
        IKubeLogService logService,
        ILlmClient llmClient,
        IKubeClusterRepository clusterRepository)
    {
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        _llmClient = llmClient ?? throw new ArgumentNullException(nameof(llmClient));
        _clusterRepository = clusterRepository ?? throw new ArgumentNullException(nameof(clusterRepository));
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
            var analysisResult = await AnalyzeLogsAsync(clusterId, logResult.Logs, cancellationToken);

            return new AnalysisResponse
            {
                Success = true,
                Data = analysisResult
            };
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
            var analysisResult = await AnalyzeLogsAsync(clusterId, logResult.Logs, cancellationToken);

            return new AnalysisResponse
            {
                Success = true,
                Data = analysisResult
            };
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

    private async Task<AnalysisResult> AnalyzeLogsAsync(Guid clusterId, string logs, CancellationToken cancellationToken)
    {
        // Get cluster configuration
        var cluster = await _clusterRepository.GetByIdAsync(clusterId);

        // Build comprehensive instructions
        var baseInstructions = cluster?.Instructions ??
            "Analyze these Kubernetes logs and identify root-cause errors. Focus on error-level messages and provide actionable solutions.";

        var jsonSchema = cluster?.ResponseFormat ??
            @"{
  ""errors"": [
    {
      ""general_message"": ""short description of the error"",
      ""occurrences"": [""original error line 1"", ""original error line 2""],
      ""solutions"": [
        {
          ""description"": ""solution description"",
          ""steps"": [
            {
              ""title"": ""step title"",
              ""explanation"": ""step explanation"",
              ""command"": ""kubectl or helm command""
            }
          ]
        }
      ]
    }
  ]
}";

        // Combine instructions with format and guidelines
        var instructions = $@"{baseInstructions}

Return a JSON object in this format: {jsonSchema}

Important guidelines:
- Focus only on ERROR, FATAL, or similar error-level messages
- Ignore WARNING or INFO messages unless they indicate real issues
- Deduplicate similar errors and group them logically
- Provide specific, actionable kubectl/helm commands in solutions
- Keep error messages concise but meaningful
- If no errors found, return an empty errors array";

        // Call LLM
        var llmResponse = await _llmClient.AnalyzeLogsAsync(logs, instructions, cancellationToken);

        // Clean LLM response (remove markdown code blocks, etc.)
        var cleanResponse = CleanLlmResponse(llmResponse);

        // Parse and validate response
        try
        {
            var result = JsonSerializer.Deserialize<AnalysisResult>(cleanResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Validate structure (basic validation)
            if (result == null || result.Errors == null)
            {
                throw new JsonException("Invalid response structure");
            }

            return result;
        }
        catch (JsonException ex)
        {
            throw new LlmAnalysisException($"Failed to parse LLM response as JSON: {ex.Message}", ex);
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
