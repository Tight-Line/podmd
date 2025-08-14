using System.ComponentModel.DataAnnotations;
using PodMD.Api.Extensions;

namespace PodMD.Api.Config;

public class DataProtectionConfig : IOptionsConfig
{
    public static string Section { get; set; } = "DataProtection";

    [Required] public required string TokenEncryptionKey { get; init; }
}