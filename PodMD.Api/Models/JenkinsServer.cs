namespace PodMD.Api.Models;

public class JenkinsServer : Configuration
{
    public required string Username { get; set; }
    public required string ApiToken { get; set; }
}