using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PodMD.Domain.Entities;

namespace PodMD.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ApiKey> ApiKeys { get; set; }
    public DbSet<KubeCluster> KubeClusters { get; set; }
    public DbSet<JenkinsServers> JenkinsServers { get; set; }
    public DbSet<KnowledgeBase> KnowledgeBases { get; set; }
    public DbSet<KnowledgeFile> KnowledgeFiles { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply configurations
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Configure TPT inheritance for Source hierarchy
        builder.Entity<Source>().HasKey(s => s.Id);
        builder.Entity<Source>().Property(s => s.Type).HasMaxLength(50).IsRequired();
        builder.Entity<Source>().Property(s => s.Name).HasMaxLength(100).IsRequired();
        builder.Entity<Source>().Property(s => s.Server).IsRequired();
        builder.Entity<Source>().Property(s => s.KeyVersion).IsRequired();
        builder.Entity<Source>().Property(s => s.CreatedAt).IsRequired();
        builder.Entity<Source>().Property(s => s.UpdatedAt).IsRequired();
        builder.Entity<Source>().ToTable("Sources");

        builder.Entity<KubeCluster>().ToTable("KubeClusters");
        builder.Entity<KubeCluster>().Property(c => c.BearerTokenEnc).IsRequired();
        builder.Entity<KubeCluster>().Property(c => c.InsecureSkipTlsVerify).IsRequired();

        builder.Entity<JenkinsServers>().ToTable("JenkinsServers");
        builder.Entity<JenkinsServers>().Property(s => s.Username).HasMaxLength(100).IsRequired();
        builder.Entity<JenkinsServers>().Property(s => s.ApiTokenEnc).IsRequired();

        // Configure KnowledgeBase entity
        builder.Entity<KnowledgeBase>().ToTable("KnowledgeBases");
        builder.Entity<KnowledgeBase>().Property(kb => kb.Name).HasMaxLength(100).IsRequired();
        builder.Entity<KnowledgeBase>().Property(kb => kb.Description).HasMaxLength(1000);
        builder.Entity<KnowledgeBase>().Property(kb => kb.CreatedAt).IsRequired();
        builder.Entity<KnowledgeBase>().Property(kb => kb.UpdatedAt).IsRequired();

        // Configure many-to-many relationship between KnowledgeBase and Source
        // EF Core will automatically create the KnowledgeBaseSource junction table
        builder.Entity<KnowledgeBase>()
            .HasMany(kb => kb.Sources)
            .WithMany(s => s.KnowledgeBases)
            .UsingEntity(j => j.ToTable("KnowledgeBaseSource"));

        // Configure KnowledgeFile entity
        builder.Entity<KnowledgeFile>().ToTable("KnowledgeFiles");
        builder.Entity<KnowledgeFile>().Property(kf => kf.FileName).HasMaxLength(255).IsRequired();
        builder.Entity<KnowledgeFile>().Property(kf => kf.StorageKey).HasMaxLength(500).IsRequired();
        builder.Entity<KnowledgeFile>().Property(kf => kf.ContentType).HasMaxLength(100).IsRequired();
        builder.Entity<KnowledgeFile>().Property(kf => kf.FileSize).IsRequired();
        builder.Entity<KnowledgeFile>().Property(kf => kf.CreatedAt).IsRequired();
        builder.Entity<KnowledgeFile>().Property(kf => kf.UpdatedAt).IsRequired();
        builder.Entity<KnowledgeFile>().Property(kf => kf.IsDeleted).IsRequired().HasDefaultValue(false);

        // Foreign key to KnowledgeBase with CASCADE delete
        builder.Entity<KnowledgeFile>()
            .HasOne(kf => kf.KnowledgeBase)
            .WithMany()
            .HasForeignKey(kf => kf.KnowledgeBaseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint: only one non-deleted file with same name per KnowledgeBase
        builder.Entity<KnowledgeFile>()
            .HasIndex(kf => new { kf.KnowledgeBaseId, kf.FileName })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        // Configure ApiKey entity
        builder.Entity<ApiKey>().ToTable("ApiKeys");
        builder.Entity<ApiKey>().Property(k => k.UserId).IsRequired();
        builder.Entity<ApiKey>().Property(k => k.HashedKey).HasMaxLength(64).IsRequired();
        builder.Entity<ApiKey>().Property(k => k.UsageCount).IsRequired().HasDefaultValue(0);
        builder.Entity<ApiKey>().Property(k => k.Status).HasMaxLength(20).IsRequired();
        builder.Entity<ApiKey>().Property(k => k.CreatedAt).IsRequired();
        builder.Entity<ApiKey>().HasIndex(k => k.HashedKey).IsUnique();
        builder.Entity<ApiKey>().HasIndex(k => k.UserId);

        // Foreign key to ApplicationUser with CASCADE delete
        builder.Entity<ApiKey>()
            .HasOne(k => k.User)
            .WithMany()
            .HasForeignKey(k => k.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
