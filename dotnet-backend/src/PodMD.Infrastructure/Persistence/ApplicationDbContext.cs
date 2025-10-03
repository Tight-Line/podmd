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

    public DbSet<KubeCluster> KubeClusters { get; set; }
    public DbSet<JenkinsServers> JenkinsServers { get; set; }
    public DbSet<KnowledgeBase> KnowledgeBases { get; set; }

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
    }
}
