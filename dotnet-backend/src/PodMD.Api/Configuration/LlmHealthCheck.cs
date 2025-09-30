using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PodMD.Api.Configuration;

public class LlmHealthCheck : IHealthCheck
{
    private readonly PodMD.Application.Analysis.ILlmClient _llmClient;

    public LlmHealthCheck(PodMD.Application.Analysis.ILlmClient llmClient)
    {
        _llmClient = llmClient ?? throw new ArgumentNullException(nameof(llmClient));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var isAvailable = await _llmClient.IsServiceAvailableAsync(cancellationToken);

            if (!isAvailable)
            {
                return HealthCheckResult.Degraded("LLM service is not available but system can still function");
            }

            return HealthCheckResult.Healthy("LLM service is available");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded($"LLM service health check failed: {ex.Message}");
        }
    }
}
