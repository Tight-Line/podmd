namespace PodMD.Api.Requests;

public record CreateClusterRequest(string Name, string Host, string Token, string? CaCertPem);