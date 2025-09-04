namespace PodMD.Api.Requests;

public record DiagnoseClusterPodRequest(
    string Namespace,
    string Pod,
    int? Tail,
    DateTimeOffset? SinceTime,
    IEnumerable<Guid>? KnowledgeBaseGuids);