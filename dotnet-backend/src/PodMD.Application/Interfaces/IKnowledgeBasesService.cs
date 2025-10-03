using PodMD.Application.Dtos;

namespace PodMD.Application.Interfaces;

public interface IKnowledgeBasesService
{
    Task<KnowledgeBaseWithSourcesDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<KnowledgeBaseDto>> GetAllAsync();
    Task<KnowledgeBaseDto> CreateAsync(CreateKnowledgeBaseDto request);
    Task<KnowledgeBaseDto> UpdateAsync(Guid id, UpdateKnowledgeBaseDto request);
    Task DeleteAsync(Guid id);

    // Relationship management methods
    Task AddSourceToKnowledgeBaseAsync(Guid knowledgeBaseId, Guid sourceId);
    Task RemoveSourceFromKnowledgeBaseAsync(Guid knowledgeBaseId, Guid sourceId);
    Task<IEnumerable<SourceReadDto>> GetSourcesForKnowledgeBaseAsync(Guid knowledgeBaseId);
    Task<IEnumerable<KnowledgeBaseDto>> GetKnowledgeBasesForSourceAsync(Guid sourceId);
}
