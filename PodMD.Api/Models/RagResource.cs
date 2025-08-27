using System.ComponentModel.DataAnnotations;

namespace PodMD.Api.Models;

public class RagResource
{
    [Key] public uint Id { get; init; }
    public required Guid Guid { get; init; }
    public required string Name { get; init; }
    public required string OpenAIFileId { get; set; }
    public required uint ApiKeyId { get; init; }
    public ICollection<Cluster> Clusters { get; set; } = new List<Cluster>();
    public ApiKey ApiKey { get; set; }
}