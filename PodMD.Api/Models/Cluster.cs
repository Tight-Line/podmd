using System.ComponentModel.DataAnnotations;

namespace PodMD.Api.Models;

public class Cluster
{
    [Key] public uint Id { get; init; }
    public required Guid Guid { get; init; }
    public required string Host { get; init; }
    public required string AccessToken { get; init; }
}