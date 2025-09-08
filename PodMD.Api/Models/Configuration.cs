using System.ComponentModel.DataAnnotations;

namespace PodMD.Api.Models;

public abstract class Configuration
{
    [Key] public uint Id { get; init; }
    public required Guid Guid { get; init; }
    public required string Name { get; set; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required uint ApiKeyId { get; init; }
    public ApiKey ApiKey { get; set; }

    public ICollection<KnowledgeBase> KnowledgeBases { get; set; } = new List<KnowledgeBase>();
}