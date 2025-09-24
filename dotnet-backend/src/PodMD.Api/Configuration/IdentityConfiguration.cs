using Microsoft.AspNetCore.Identity;
using PodMD.Domain.Entities;

namespace PodMD.Api.Configuration;

public static class IdentityConfiguration
{
    public static IServiceCollection AddIdentityConfiguration(this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;

            options.User.RequireUniqueEmail = true;

            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
        })
        .AddEntityFrameworkStores<PodMD.Infrastructure.Persistence.ApplicationDbContext>()
        .AddDefaultTokenProviders();

        return services;
    }
}
