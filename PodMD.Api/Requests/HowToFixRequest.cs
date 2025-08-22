namespace PodMD.Api.Requests;

public record HowToFixRequest(string Namespace, string Pod, int? Tail, DateTimeOffset? SinceTime);