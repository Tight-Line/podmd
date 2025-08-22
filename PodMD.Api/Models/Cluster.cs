using System.ComponentModel.DataAnnotations;

namespace PodMD.Api.Models;

public class Cluster
{
    [Key] public uint Id { get; init; }
    public required Guid Guid { get; init; }
    public required string Host { get; set; }
    public required string AccessToken { get; set; }
    public string? OpenAIAssistantId { get; set; }
    public ICollection<RagResource> RagResources { get; set; } = new List<RagResource>();
}