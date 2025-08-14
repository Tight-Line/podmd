using Microsoft.Extensions.Options;

namespace PodMD.Api.Extensions;

public interface IOptionsConfig
{
    public static abstract string Section { get; set; }
}

public static class OptionsExtensions
{
    private static OptionsBuilder<T> ValidateOnStartIfProd<T>(this OptionsBuilder<T> builder,
        IWebHostEnvironment env)
        where T : class
    {
        return env.IsProduction() ? builder.ValidateOnStart() : builder;
    }

    public static OptionsBuilder<TConfig> AddConfig<TConfig>(IServiceCollection services, IWebHostEnvironment env)
        where TConfig : class, IOptionsConfig
    {
        return services.AddOptions<TConfig>().BindConfiguration(TConfig.Section).ValidateDataAnnotations()
            .ValidateOnStartIfProd(env);
    }

    public static OptionsBuilder<TConfig> AddConfig<TConfig>(this WebApplicationBuilder builder)
        where TConfig : class, IOptionsConfig
    {
        return builder.Services.AddOptions<TConfig>().Bind(builder.Configuration.GetSection(TConfig.Section))
            .ValidateDataAnnotations()
            .ValidateOnStartIfProd(builder.Environment);
    }


    public static OptionsBuilder<TConfig> AddConfigRoot<TConfig>(this WebApplicationBuilder builder)
        where TConfig : class
    {
        return builder.Services.AddOptions<TConfig>().Bind(builder.Configuration).ValidateDataAnnotations()
            .ValidateOnStartIfProd(builder.Environment);
    }
}