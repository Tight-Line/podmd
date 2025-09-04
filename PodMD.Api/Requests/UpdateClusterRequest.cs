namespace PodMD.Api.Requests;

public record UpdateClusterRequest(string Name, string Host, string Token, string? CaCertPem);