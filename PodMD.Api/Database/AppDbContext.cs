using Microsoft.EntityFrameworkCore;
using PodMD.Api.Models;

namespace PodMD.Api.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Cluster> Clusters { get; set; }
    public DbSet<KnowledgeBase> KnowledgeBases { get; set; }
    public DbSet<Resource> Resources { get; set; }
    public DbSet<ApiKey> ApiKeys { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Cluster>()
            .HasMany(c => c.KnowledgeBases)
            .WithMany(kb => kb.Clusters)
            .UsingEntity<Dictionary<string, object>>(
                "ClusterKnowledgeBase",
                j => j
                    .HasOne<KnowledgeBase>()
                    .WithMany()
                    .HasForeignKey("KnowledgeBaseId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j
                    .HasOne<Cluster>()
                    .WithMany()
                    .HasForeignKey("ClusterId")
                    .OnDelete(DeleteBehavior.Cascade)
            );
    }
}