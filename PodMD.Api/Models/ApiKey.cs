namespace PodMD.Api.Models;

public class ApiKey
{
    public uint Id { get; init; }
    public required string LookupName { get; init; }
    public required string SecretHash { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public ICollection<Cluster> Clusters { get; set; } = new List<Cluster>();
    public ICollection<RagResource> RagResources { get; set; } = new List<RagResource>();
}