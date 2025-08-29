using System.ComponentModel.DataAnnotations;

namespace PodMD.Api.Models;

public class Cluster
{
    [Key] public uint Id { get; init; }
    public required Guid Guid { get; init; }
    public required string Host { get; set; }
    public required string AccessToken { get; set; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required uint ApiKeyId { get; init; }
    public ApiKey ApiKey { get; set; }

    public ICollection<KnowledgeBase> KnowledgeBases { get; set; } = new List<KnowledgeBase>();
}