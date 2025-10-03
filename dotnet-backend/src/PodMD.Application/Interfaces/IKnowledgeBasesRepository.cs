using PodMD.Domain.Entities;

namespace PodMD.Application.Interfaces;

public interface IKnowledgeBasesRepository
{
    Task<KnowledgeBase?> GetByIdAsync(Guid id);
    Task<IEnumerable<KnowledgeBase>> GetAllAsync();
    Task<KnowledgeBase> AddAsync(KnowledgeBase knowledgeBase);
    Task UpdateAsync(KnowledgeBase knowledgeBase);
    Task DeleteAsync(KnowledgeBase knowledgeBase);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> SourceExistsAsync(Guid sourceId);

    // Relationship management methods
    Task<IEnumerable<Source>> GetSourcesForKnowledgeBaseAsync(Guid knowledgeBaseId);
    Task<IEnumerable<KnowledgeBase>> GetKnowledgeBasesForSourceAsync(Guid sourceId);
    Task AddSourceToKnowledgeBaseAsync(Guid knowledgeBaseId, Guid sourceId);
    Task RemoveSourceFromKnowledgeBaseAsync(Guid knowledgeBaseId, Guid sourceId);
}
