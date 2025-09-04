namespace PodMD.Api.Requests;

public record CreateClusterRequest(string Host, string Token, string? CaCertPem);