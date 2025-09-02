using Microsoft.EntityFrameworkCore;
using PodMD.Api.Models;

namespace PodMD.Api.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Configuration> Configurations { get; set; }
    public DbSet<Cluster> Clusters { get; set; }
    public DbSet<KnowledgeBase> KnowledgeBases { get; set; }
    public DbSet<Resource> Resources { get; set; }
    public DbSet<ApiKey> ApiKeys { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Configuration>().ToTable("Configurations");
        modelBuilder.Entity<Cluster>().ToTable("Clusters");

        modelBuilder.Entity<Configuration>()
            .HasMany(c => c.KnowledgeBases)
            .WithMany(kb => kb.Configurations)
            .UsingEntity<Dictionary<string, object>>(
                "ConfigurationKnowledgeBase",
                j => j
                    .HasOne<KnowledgeBase>()
                    .WithMany()
                    .HasForeignKey("KnowledgeBaseId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j
                    .HasOne<Configuration>()
                    .WithMany()
                    .HasForeignKey("ConfigurationId")
                    .OnDelete(DeleteBehavior.Cascade)
            );

        modelBuilder.Entity<Configuration>()
            .HasOne(c => c.ApiKey)
            .WithMany(a => a.Configurations)
            .HasForeignKey(c => c.ApiKeyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<KnowledgeBase>()
            .HasOne(k => k.ApiKey)
            .WithMany(a => a.KnowledgeBases)
            .HasForeignKey(k => k.ApiKeyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Resource>()
            .HasOne(r => r.ApiKey)
            .WithMany(a => a.Resources)
            .HasForeignKey(r => r.ApiKeyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Resource>()
            .HasOne(r => r.KnowledgeBase)
            .WithMany(kb => kb.Resources)
            .HasForeignKey(r => r.KnowledgeBaseId)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}