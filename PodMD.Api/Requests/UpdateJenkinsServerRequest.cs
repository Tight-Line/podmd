namespace PodMD.Api.Requests;

public record UpdateJenkinsServerRequest(string Name, string Host, string Token, string Username);