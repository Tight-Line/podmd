namespace PodMD.Api.Responses;

public record ConfigurationResponse(Guid Guid, string Name, string Host, string Type, DateTimeOffset CreatedAt);