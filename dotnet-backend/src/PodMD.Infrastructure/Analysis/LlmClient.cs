using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;
using PodMD.Application.Analysis;
using PodMD.Application.Configuration;

namespace PodMD.Infrastructure.Analysis;

public class OpenAiApiRequest
{
    public string model { get; set; } = "gpt-3.5-turbo";
    public List<OpenAiMessage> messages { get; set; } = new();
    public double temperature { get; set; } = 0.1;
    public int max_tokens { get; set; } = 2048;
}

public class OpenAiMessage
{
    public string role { get; set; } = string.Empty;
    public string content { get; set; } = string.Empty;
}

public class OpenAiApiResponse
{
    public List<OpenAiChoice> choices { get; set; } = new();
    public OpenAiUsage usage { get; set; } = new();
    public string? error { get; set; }
}

public class OpenAiChoice
{
    public OpenAiMessage message { get; set; } = new();
    public string finish_reason { get; set; } = string.Empty;
}

public class OpenAiUsage
{
    public int prompt_tokens { get; set; }
    public int completion_tokens { get; set; }
    public int total_tokens { get; set; }
}

public class LlmClient : ILlmClient, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly LlmSettings _settings;
    private readonly ILogger<LlmClient> _logger;

    public LlmClient(IOptions<LlmSettings> settings, ILogger<LlmClient> logger)
    {
        ArgumentNullException.ThrowIfNull(settings);
        _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _settings.Validate(logger);

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_settings.BaseUrl!),
            Timeout = TimeSpan.FromSeconds(_settings.EffectiveTimeoutSeconds)
        };

        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "PodMD/1.0");
    }

    public async Task<string> AnalyzeLogsAsync(string logs, string prompt, string? description = null, CancellationToken cancellationToken = default)
    {
        // Prepend description to logs if available
        var fullLogs = string.IsNullOrWhiteSpace(description) ? logs : $"{description}\n\n{logs}";



        var fullPrompt = $@"{prompt}

Log content to analyze:
{fullLogs}";

        var request = new OpenAiApiRequest
        {
            model = _settings.Model!,
            messages = new List<OpenAiMessage>
            {
                new() { role = "user", content = fullPrompt }
            }
        };

        if (_settings.MaxTokens.HasValue)
            request.max_tokens = _settings.MaxTokens.Value;

        if (_settings.Temperature.HasValue)
            request.temperature = _settings.Temperature.Value;

        var retryCount = 0;
        while (retryCount < _settings.EffectiveMaxRetries)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("chat/completions", request, cancellationToken);
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new LlmAnalysisException(
                        $"LLM API request failed with status {response.StatusCode}: {responseContent}",
                        response.StatusCode);
                }

                var apiResponse = JsonSerializer.Deserialize<OpenAiApiResponse>(responseContent);

                if (apiResponse?.error != null)
                {
                    throw new LlmAnalysisException($"LLM API error: {apiResponse.error}");
                }

                var analysis = apiResponse?.choices.FirstOrDefault()?.message.content;

                if (string.IsNullOrWhiteSpace(analysis))
                {
                    throw new LlmAnalysisException("LLM returned empty response");
                }

                return analysis;
            }
            catch (HttpRequestException ex) when (ex.StatusCode is System.Net.HttpStatusCode.TooManyRequests)
            {
                retryCount++;
                if (retryCount >= _settings.EffectiveMaxRetries)
                    throw new LlmAnalysisException("Rate limit exceeded after retries", ex);

                var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount)); // Exponential backoff
                await Task.Delay(delay, cancellationToken);
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                retryCount++;
                if (retryCount >= _settings.EffectiveMaxRetries)
                    throw new LlmAnalysisException("LLM request timed out after retries");

                await Task.Delay(TimeSpan.FromSeconds(_settings.EffectiveTimeoutSeconds), cancellationToken);
            }
            catch (Exception ex) when (ex is not LlmAnalysisException)
            {
                throw new LlmAnalysisException("Unexpected error during LLM analysis", ex);
            }
        }

        throw new LlmAnalysisException("Max retries exceeded");
    }

    public async Task<bool> IsServiceAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("models", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
