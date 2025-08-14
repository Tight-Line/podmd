using System.ComponentModel.DataAnnotations;
using PodMD.Api.Extensions;

namespace PodMD.Api.Config;

public class OpenAIConfig : IOptionsConfig
{
    public static string Section { get; set; } = "OpenAI";

    [Required] public required string ApiKey { get; init; }
    [Required] public required string BaseUrl { get; init; }
    [Required] public required string Model { get; init; }
}