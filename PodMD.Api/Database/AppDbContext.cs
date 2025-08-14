using Microsoft.EntityFrameworkCore;
using PodMD.Api.Models;

namespace PodMD.Api.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Cluster> Clusters { get; set; }
}