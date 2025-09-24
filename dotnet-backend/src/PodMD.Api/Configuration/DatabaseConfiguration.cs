using Microsoft.EntityFrameworkCore;
using PodMD.Application.Configuration;
using PodMD.Infrastructure.Persistence;

namespace PodMD.Api.Configuration;

public static class DatabaseConfiguration
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseSettings = configuration.GetSection("Database").Get<DatabaseSettings>()
            ?? throw new InvalidOperationException("Database settings are not configured");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseMySql(
                databaseSettings.ConnectionString,
                ServerVersion.AutoDetect(databaseSettings.ConnectionString),
                mysqlOptions =>
                {
                    mysqlOptions.EnableRetryOnFailure(
                        maxRetryCount: databaseSettings.MaxRetryCount,
                        maxRetryDelay: databaseSettings.MaxRetryDelay,
                        errorNumbersToAdd: null);
                });
        });

        return services;
    }
}
