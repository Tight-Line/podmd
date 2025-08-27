using Microsoft.EntityFrameworkCore;
using PodMD.Api.Models;

namespace PodMD.Api.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Cluster> Clusters { get; set; }
    public DbSet<RagResource> RagResources { get; set; }
    public DbSet<ApiKey> ApiKeys { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Cluster>()
            .HasMany(c => c.RagResources)
            .WithMany(r => r.Clusters)
            .UsingEntity<Dictionary<string, object>>(
                "ClusterRagResource",
                j => j.HasOne<RagResource>().WithMany().HasForeignKey("RagResourceId"),
                j => j.HasOne<Cluster>().WithMany().HasForeignKey("ClusterId")
            );
    }
}