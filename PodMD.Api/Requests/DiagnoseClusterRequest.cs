namespace PodMD.Api.Requests;

public record DiagnoseClusterRequest(string Namespace, string Pod, int? Tail, DateTimeOffset? SinceTime);