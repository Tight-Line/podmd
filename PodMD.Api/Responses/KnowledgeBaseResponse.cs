namespace PodMD.Api.Responses;

public record KnowledgeBaseResponse(Guid Guid, string Name, string Description, DateTimeOffset CreatedAt);