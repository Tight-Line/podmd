using PodMD.Domain.Entities;

namespace PodMD.Application.Interfaces;

public interface IKnowledgeFileRepository
{
    Task<KnowledgeFile?> GetByIdAsync(Guid id);
    Task<IEnumerable<KnowledgeFile>> GetByKnowledgeBaseIdAsync(Guid knowledgeBaseId);
    Task<KnowledgeFile?> GetByKnowledgeBaseIdAndFileNameAsync(Guid knowledgeBaseId, string fileName);
    Task<KnowledgeFile> CreateAsync(KnowledgeFile knowledgeFile);
    Task<KnowledgeFile> UpdateAsync(KnowledgeFile knowledgeFile);
    Task DeleteAsync(Guid id);
}
