using Microsoft.EntityFrameworkCore;
using PodMD.Api.Database;

namespace PodMD.Api.Extensions;

public static class Migrate
{
    public static void UseEnvironmentSettings(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseOpenApi();
            app.UseSwaggerUi();
        }

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
}