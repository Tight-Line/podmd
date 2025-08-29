using System.ComponentModel.DataAnnotations;

namespace PodMD.Api.Models;

public class Resource
{
    [Key] public uint Id { get; init; }
    public required Guid Guid { get; init; }
    public required string FileName { get; init; }
    public required string OpenAIFileId { get; set; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required uint ApiKeyId { get; init; }
    public ApiKey ApiKey { get; set; }
    public required uint KnowledgeBaseId { get; set; }
    public KnowledgeBase KnowledgeBase { get; set; }
}