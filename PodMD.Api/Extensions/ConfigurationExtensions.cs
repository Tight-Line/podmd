using PodMD.Api.Config;

namespace PodMD.Api.Extensions;

public static class ConfigurationExtensions
{
    public static void RegisterConfigs(this WebApplicationBuilder builder)
    {
        builder.AddConfig<OpenAIConfig>();
        builder.AddConfig<DataProtectionConfig>();
    }
}