namespace PodMD.Api.Responses;

public record ClusterResponse(Guid Guid, string Name, string Host, DateTimeOffset CreatedAt);