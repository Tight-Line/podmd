using System.ComponentModel.DataAnnotations;

namespace PodMD.Api.Models;

public class RagResource
{
    [Key] public uint Id { get; init; }
    public required Guid Guid { get; init; }
    public required string Name { get; set; }
    public required string OpenAIFileId { get; set; }
    public ICollection<Cluster> Clusters { get; set; } = new List<Cluster>();
}