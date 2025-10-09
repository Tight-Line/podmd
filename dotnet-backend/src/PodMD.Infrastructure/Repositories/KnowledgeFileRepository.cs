using Microsoft.EntityFrameworkCore;
using PodMD.Application.Interfaces;
using PodMD.Domain.Entities;
using PodMD.Infrastructure.Persistence;

namespace PodMD.Infrastructure.Repositories;

public class KnowledgeFileRepository : IKnowledgeFileRepository
{
    private readonly ApplicationDbContext _context;

    public KnowledgeFileRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<KnowledgeFile?> GetByIdAsync(Guid id)
    {
        return await _context.KnowledgeFiles
            .Include(kf => kf.KnowledgeBase)
            .FirstOrDefaultAsync(kf => kf.Id == id);
    }

    public async Task<IEnumerable<KnowledgeFile>> GetByKnowledgeBaseIdAsync(Guid knowledgeBaseId)
    {
        return await _context.KnowledgeFiles
            .Include(kf => kf.KnowledgeBase)
            .Where(kf => kf.KnowledgeBaseId == knowledgeBaseId)
            .OrderBy(kf => kf.CreatedAt)
            .ToListAsync();
    }

    public async Task<KnowledgeFile?> GetByKnowledgeBaseIdAndFileNameAsync(Guid knowledgeBaseId, string fileName)
    {
        return await _context.KnowledgeFiles
            .FirstOrDefaultAsync(kf => kf.KnowledgeBaseId == knowledgeBaseId && kf.FileName == fileName);
    }

    public async Task<KnowledgeFile> CreateAsync(KnowledgeFile knowledgeFile)
    {
        _context.KnowledgeFiles.Add(knowledgeFile);
        await _context.SaveChangesAsync();
        return knowledgeFile;
    }

    public async Task<KnowledgeFile> UpdateAsync(KnowledgeFile knowledgeFile)
    {
        _context.KnowledgeFiles.Update(knowledgeFile);
        await _context.SaveChangesAsync();
        return knowledgeFile;
    }

    public async Task DeleteAsync(Guid id)
    {
        var knowledgeFile = await _context.KnowledgeFiles.FindAsync(id);
        if (knowledgeFile == null)
        {
            throw new KeyNotFoundException($"Knowledge file with ID '{id}' not found.");
        }

        _context.KnowledgeFiles.Remove(knowledgeFile);
        await _context.SaveChangesAsync();
    }


}
