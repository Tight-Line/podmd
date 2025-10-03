using Microsoft.EntityFrameworkCore;
using PodMD.Application.Interfaces;
using PodMD.Domain.Entities;
using PodMD.Infrastructure.Persistence;

namespace PodMD.Infrastructure.Repositories;

public class KnowledgeBasesRepository : IKnowledgeBasesRepository
{
    private readonly ApplicationDbContext _context;

    public KnowledgeBasesRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<KnowledgeBase?> GetByIdAsync(Guid id)
    {
        return await _context.KnowledgeBases
            .Include(kb => kb.Sources)
            .FirstOrDefaultAsync(kb => kb.Id == id);
    }

    public async Task<IEnumerable<KnowledgeBase>> GetAllAsync()
    {
        return await _context.KnowledgeBases
            .Include(kb => kb.Sources)
            .OrderBy(kb => kb.Name)
            .ToListAsync();
    }

    public async Task<KnowledgeBase> AddAsync(KnowledgeBase knowledgeBase)
    {
        _context.KnowledgeBases.Add(knowledgeBase);
        await _context.SaveChangesAsync();
        return knowledgeBase;
    }

    public async Task UpdateAsync(KnowledgeBase knowledgeBase)
    {
        _context.KnowledgeBases.Update(knowledgeBase);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(KnowledgeBase knowledgeBase)
    {
        // Remove all source associations before deleting (detach behavior)
        knowledgeBase.Sources.Clear();
        _context.KnowledgeBases.Update(knowledgeBase);
        await _context.SaveChangesAsync();

        _context.KnowledgeBases.Remove(knowledgeBase);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.KnowledgeBases.AnyAsync(kb => kb.Id == id);
    }

    public async Task<bool> SourceExistsAsync(Guid sourceId)
    {
        return await _context.Set<Source>().AnyAsync(s => s.Id == sourceId);
    }

    public async Task<IEnumerable<Source>> GetSourcesForKnowledgeBaseAsync(Guid knowledgeBaseId)
    {
        var knowledgeBase = await _context.KnowledgeBases
            .Include(kb => kb.Sources)
            .FirstOrDefaultAsync(kb => kb.Id == knowledgeBaseId);

        return knowledgeBase?.Sources ?? new List<Source>();
    }

    public async Task<IEnumerable<KnowledgeBase>> GetKnowledgeBasesForSourceAsync(Guid sourceId)
    {
        var source = await _context.Set<Source>()
            .Include(s => s.KnowledgeBases)
            .FirstOrDefaultAsync(s => s.Id == sourceId);

        return source?.KnowledgeBases ?? new List<KnowledgeBase>();
    }

    public async Task AddSourceToKnowledgeBaseAsync(Guid knowledgeBaseId, Guid sourceId)
    {
        var knowledgeBase = await _context.KnowledgeBases
            .Include(kb => kb.Sources)
            .FirstOrDefaultAsync(kb => kb.Id == knowledgeBaseId);

        if (knowledgeBase == null)
        {
            throw new KeyNotFoundException($"Knowledge base with ID '{knowledgeBaseId}' not found.");
        }

        var source = await _context.Set<Source>().FindAsync(sourceId);
        if (source == null)
        {
            throw new KeyNotFoundException($"Source with ID '{sourceId}' not found.");
        }

        // Check if association already exists
        if (knowledgeBase.Sources.Any(s => s.Id == sourceId))
        {
            return; // Already associated
        }

        knowledgeBase.Sources.Add(source);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveSourceFromKnowledgeBaseAsync(Guid knowledgeBaseId, Guid sourceId)
    {
        var knowledgeBase = await _context.KnowledgeBases
            .Include(kb => kb.Sources)
            .FirstOrDefaultAsync(kb => kb.Id == knowledgeBaseId);

        if (knowledgeBase == null)
        {
            throw new KeyNotFoundException($"Knowledge base with ID '{knowledgeBaseId}' not found.");
        }

        var source = knowledgeBase.Sources.FirstOrDefault(s => s.Id == sourceId);
        if (source == null)
        {
            return; // Not associated
        }

        knowledgeBase.Sources.Remove(source);
        await _context.SaveChangesAsync();
    }
}
