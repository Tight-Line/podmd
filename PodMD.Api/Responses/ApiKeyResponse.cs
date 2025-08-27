namespace PodMD.Api.Responses;

public record ApiKeyResponse(string ApiKey, string LookupName, DateTimeOffset CreatedAt);