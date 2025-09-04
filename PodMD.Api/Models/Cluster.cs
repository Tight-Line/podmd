namespace PodMD.Api.Models;

public class Cluster : Configuration
{
    public required string Host { get; set; }
    public required string AccessToken { get; set; }
    public string? CaCertPem { get; set; }
}