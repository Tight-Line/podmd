using Microsoft.EntityFrameworkCore;
using PodMD.Application.Interfaces;
using PodMD.Domain.Entities;
using PodMD.Infrastructure.Persistence;

namespace PodMD.Infrastructure.Repositories;

public class JenkinsServersRepository : IJenkinsServersRepository
{
    private readonly ApplicationDbContext _context;

    public JenkinsServersRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<JenkinsServers?> GetByIdAsync(Guid id)
    {
        return await _context.JenkinsServers.FindAsync(id);
    }

    public async Task<IEnumerable<JenkinsServers>> GetAllAsync()
    {
        return await _context.JenkinsServers
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<JenkinsServers> AddAsync(JenkinsServers server)
    {
        _context.JenkinsServers.Add(server);
        await _context.SaveChangesAsync();
        return server;
    }

    public async Task UpdateAsync(JenkinsServers server)
    {
        _context.JenkinsServers.Update(server);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(JenkinsServers server)
    {
        _context.JenkinsServers.Remove(server);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.JenkinsServers.AnyAsync(s => s.Id == id);
    }
}
