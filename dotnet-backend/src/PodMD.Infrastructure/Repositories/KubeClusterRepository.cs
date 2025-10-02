using Microsoft.EntityFrameworkCore;
using PodMD.Application.Interfaces;
using PodMD.Domain.Entities;
using PodMD.Infrastructure.Persistence;

namespace PodMD.Infrastructure.Repositories;

public class KubeClusterRepository : IKubeClusterRepository
{
    private readonly ApplicationDbContext _context;

    public KubeClusterRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<KubeCluster?> GetByIdAsync(Guid id)
    {
        return await _context.KubeClusters.FindAsync(id);
    }



    public async Task<IEnumerable<KubeCluster>> GetAllAsync()
    {
        return await _context.KubeClusters
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<KubeCluster> AddAsync(KubeCluster cluster)
    {
        _context.KubeClusters.Add(cluster);
        await _context.SaveChangesAsync();
        return cluster;
    }

    public async Task UpdateAsync(KubeCluster cluster)
    {
        _context.KubeClusters.Update(cluster);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(KubeCluster cluster)
    {
        _context.KubeClusters.Remove(cluster);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.KubeClusters.AnyAsync(c => c.Id == id);
    }
}
