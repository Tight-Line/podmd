using System.Net;

namespace PodMD.Application.Analysis;

public interface ILlmClient
{
    Task<string> AnalyzeLogsAsync(string logs, string prompt, CancellationToken cancellationToken = default);
    Task<bool> IsServiceAvailableAsync(CancellationToken cancellationToken = default);
}

public class LlmAnalysisException : Exception
{
    public HttpStatusCode? StatusCode { get; }

    public LlmAnalysisException(string message) : base(message) { }

    public LlmAnalysisException(string message, HttpStatusCode statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    public LlmAnalysisException(string message, Exception innerException) : base(message, innerException) { }
}
