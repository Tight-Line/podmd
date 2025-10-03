using PodMD.Application.Dtos;

namespace PodMD.Application.Interfaces;

public interface IKnowledgeFileService
{
    Task<IEnumerable<KnowledgeFileDto>> UploadFilesAsync(IEnumerable<CreateKnowledgeFileDto> uploadRequests);
    Task<IEnumerable<KnowledgeFileDto>> GetByKnowledgeBaseIdAsync(Guid knowledgeBaseId);
    Task<KnowledgeFileDto?> GetByIdAsync(Guid id);
    Task<KnowledgeFileDto> ReplaceFileAsync(Guid fileId, UpdateKnowledgeFileDto request);
    Task DeleteAsync(Guid id);
}
