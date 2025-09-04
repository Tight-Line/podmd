namespace PodMD.Api.Requests;

public record AdhocDiagnosePodRequest(
    string Host,
    string Token,
    string Namespace,
    string Pod,
    int? Tail,
    DateTimeOffset? SinceTime,
    IEnumerable<Guid> KnowledgeBaseGuids) : DiagnoseClusterPodRequest(Namespace, Pod, Tail, SinceTime, KnowledgeBaseGuids);