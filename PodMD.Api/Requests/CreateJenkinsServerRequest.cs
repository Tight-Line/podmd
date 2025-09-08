namespace PodMD.Api.Requests;

public record CreateJenkinsServerRequest(string Name, string Host, string ApiToken, string Username);