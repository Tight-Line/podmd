using Microsoft.Extensions.Logging;

namespace PodMD.Application.Configuration;

public class LlmSettings
{
    public string? ApiKey { get; set; }
    public string? BaseUrl { get; set; }
    public string? Model { get; set; }
    public int? MaxRetries { get; set; }
    public int? TimeoutSeconds { get; set; }
    public int? MaxTokens { get; set; }
    public double? Temperature { get; set; }

    public int EffectiveMaxRetries => MaxRetries ?? 0;
    public int EffectiveTimeoutSeconds => TimeoutSeconds ?? 300;

    public void Validate(ILogger logger)
    {
        // Required values - throw if missing
        if (string.IsNullOrWhiteSpace(ApiKey))
            throw new ArgumentException("LLM API key is required", nameof(ApiKey));
        if (string.IsNullOrWhiteSpace(BaseUrl))
            throw new ArgumentException("LLM BaseUrl is required", nameof(BaseUrl));
        if (string.IsNullOrWhiteSpace(Model))
            throw new ArgumentException("LLM Model is required", nameof(Model));

        // Optional values - log warnings if missing (but don't set defaults here, use Effective* properties)
        if (!MaxRetries.HasValue)
            logger.LogWarning("LLM MaxRetries not configured, using default of 0 retries");
        if (!TimeoutSeconds.HasValue)
            logger.LogWarning("LLM TimeoutSeconds not configured, using default of 300 seconds (5 minutes)");
        if (!MaxTokens.HasValue)
            logger.LogWarning("LLM MaxTokens not configured");
        if (!Temperature.HasValue)
            logger.LogWarning("LLM Temperature not configured");
    }
}
