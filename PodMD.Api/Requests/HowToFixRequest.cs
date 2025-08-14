namespace PodMD.Api.Requests;

public record HowToFixRequest(Guid ClusterGuid, string Namespace, string Pod, int? Tail, DateTimeOffset? SinceTime);