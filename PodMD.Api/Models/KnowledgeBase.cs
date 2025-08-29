using System.ComponentModel.DataAnnotations;

namespace PodMD.Api.Models;

public class KnowledgeBase
{
    [Key] public uint Id { get; init; }
    public required Guid Guid { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string OpenAIVectorStoreId { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required uint ApiKeyId { get; init; }
    public ApiKey ApiKey { get; set; }

    public ICollection<Cluster> Clusters { get; set; } = new List<Cluster>();
    public ICollection<Resource> Resources { get; set; } = new List<Resource>();
}