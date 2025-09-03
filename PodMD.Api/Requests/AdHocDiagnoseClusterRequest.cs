namespace PodMD.Api.Requests;

public record AdHocDiagnoseClusterRequest(
    string Host,
    string Token,
    string Namespace,
    string Pod,
    int? Tail,
    DateTimeOffset? SinceTime) : DiagnoseClusterRequest(Namespace, Pod, Tail, SinceTime);