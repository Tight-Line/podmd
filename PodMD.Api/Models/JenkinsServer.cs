namespace PodMD.Api.Models;

public class JenkinsServer : Configuration
{
    public required string Host { get; set; }
    public required string Username { get; set; }
    public required string ApiToken { get; set; }
}