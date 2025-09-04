namespace PodMD.Api.Requests;

public record DiagnoseLogsRequest(string Logs, IEnumerable<Guid>? KnowledgeBaseGuids);