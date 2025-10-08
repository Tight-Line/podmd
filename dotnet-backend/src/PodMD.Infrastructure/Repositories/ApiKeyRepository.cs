using Microsoft.EntityFrameworkCore;
using PodMD.Application.Interfaces;
using PodMD.Domain.Entities;
using PodMD.Infrastructure.Persistence;

namespace PodMD.Infrastructure.Repositories;

public class ApiKeyRepository : IApiKeyRepository
{
    private readonly ApplicationDbContext _context;

    public ApiKeyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiKey?> GetByIdAsync(Guid id)
    {
        return await _context.ApiKeys
            .Include(k => k.User)
            .FirstOrDefaultAsync(k => k.Id == id);
    }

    public async Task<ApiKey?> GetByHashedKeyAsync(string hashedKey)
    {
        return await _context.ApiKeys
            .Include(k => k.User)
            .FirstOrDefaultAsync(k => k.HashedKey == hashedKey);
    }

    public async Task<IEnumerable<ApiKey>> GetByUserIdAsync(string userId)
    {
        return await _context.ApiKeys
            .Include(k => k.User)
            .Where(k => k.UserId == userId)
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync();
    }

    public async Task<ApiKey> AddAsync(ApiKey apiKey)
    {
        _context.ApiKeys.Add(apiKey);
        await _context.SaveChangesAsync();
        return apiKey;
    }

    public async Task UpdateAsync(ApiKey apiKey)
    {
        _context.ApiKeys.Update(apiKey);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(ApiKey apiKey)
    {
        _context.ApiKeys.Remove(apiKey);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.ApiKeys.AnyAsync(k => k.Id == id);
    }

    public async Task<bool> ExistsAsync(string hashedKey)
    {
        return await _context.ApiKeys.AnyAsync(k => k.HashedKey == hashedKey);
    }
}
