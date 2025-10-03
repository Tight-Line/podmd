using PodMD.Application.Dtos;
using PodMD.Application.Interfaces;
using PodMD.Domain.Entities;

namespace PodMD.Application.Services;

public class KnowledgeBasesService : IKnowledgeBasesService
{
    private readonly IKnowledgeBasesRepository _repository;

    public KnowledgeBasesService(IKnowledgeBasesRepository repository)
    {
        _repository = repository;
    }

    public async Task<KnowledgeBaseWithSourcesDto?> GetByIdAsync(Guid id)
    {
        var knowledgeBase = await _repository.GetByIdAsync(id);
        if (knowledgeBase == null)
        {
            return null;
        }

        var sources = await _repository.GetSourcesForKnowledgeBaseAsync(id);
        var sourceDtos = sources.Select(MapToSourceReadDto);

        return new KnowledgeBaseWithSourcesDto(
            knowledgeBase.Id,
            knowledgeBase.Name,
            knowledgeBase.Description,
            knowledgeBase.CreatedAt,
            knowledgeBase.UpdatedAt,
            sourceDtos
        );
    }

    public async Task<IEnumerable<KnowledgeBaseDto>> GetAllAsync()
    {
        var knowledgeBases = await _repository.GetAllAsync();
        return knowledgeBases.Select(kb => new KnowledgeBaseDto(
            kb.Id,
            kb.Name,
            kb.Description,
            kb.CreatedAt,
            kb.UpdatedAt
        ));
    }

    public async Task<KnowledgeBaseDto> CreateAsync(CreateKnowledgeBaseDto request)
    {
        var knowledgeBase = new KnowledgeBase
        {
            Name = request.Name,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdKnowledgeBase = await _repository.AddAsync(knowledgeBase);

        return new KnowledgeBaseDto(
            createdKnowledgeBase.Id,
            createdKnowledgeBase.Name,
            createdKnowledgeBase.Description,
            createdKnowledgeBase.CreatedAt,
            createdKnowledgeBase.UpdatedAt
        );
    }

    public async Task<KnowledgeBaseDto> UpdateAsync(Guid id, UpdateKnowledgeBaseDto request)
    {
        var knowledgeBase = await _repository.GetByIdAsync(id);
        if (knowledgeBase == null)
        {
            throw new KeyNotFoundException($"Knowledge base with ID '{id}' not found.");
        }

        knowledgeBase.Name = request.Name;
        if (request.Description != null)
        {
            knowledgeBase.Description = request.Description;
        }
        knowledgeBase.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(knowledgeBase);

        return new KnowledgeBaseDto(
            knowledgeBase.Id,
            knowledgeBase.Name,
            knowledgeBase.Description,
            knowledgeBase.CreatedAt,
            knowledgeBase.UpdatedAt
        );
    }

    public async Task DeleteAsync(Guid id)
    {
        var knowledgeBase = await _repository.GetByIdAsync(id);
        if (knowledgeBase == null)
        {
            throw new KeyNotFoundException($"Knowledge base with ID '{id}' not found.");
        }

        await _repository.DeleteAsync(knowledgeBase);
    }

    public async Task AddSourceToKnowledgeBaseAsync(Guid knowledgeBaseId, Guid sourceId)
    {
        // Validate that both entities exist
        var knowledgeBase = await _repository.GetByIdAsync(knowledgeBaseId);
        if (knowledgeBase == null)
        {
            throw new KeyNotFoundException($"Knowledge base with ID '{knowledgeBaseId}' not found.");
        }

        // For Source validation, we'd need access to Source repository
        // For now, let the repository handle the validation and association
        await _repository.AddSourceToKnowledgeBaseAsync(knowledgeBaseId, sourceId);
    }

    public async Task RemoveSourceFromKnowledgeBaseAsync(Guid knowledgeBaseId, Guid sourceId)
    {
        var knowledgeBase = await _repository.GetByIdAsync(knowledgeBaseId);
        if (knowledgeBase == null)
        {
            throw new KeyNotFoundException($"Knowledge base with ID '{knowledgeBaseId}' not found.");
        }

        await _repository.RemoveSourceFromKnowledgeBaseAsync(knowledgeBaseId, sourceId);
    }

    public async Task<IEnumerable<SourceReadDto>> GetSourcesForKnowledgeBaseAsync(Guid knowledgeBaseId)
    {
        var knowledgeBase = await _repository.GetByIdAsync(knowledgeBaseId);
        if (knowledgeBase == null)
        {
            throw new KeyNotFoundException($"Knowledge base with ID '{knowledgeBaseId}' not found.");
        }

        var sources = await _repository.GetSourcesForKnowledgeBaseAsync(knowledgeBaseId);
        return sources.Select(MapToSourceReadDto);
    }

    public async Task<IEnumerable<KnowledgeBaseDto>> GetKnowledgeBasesForSourceAsync(Guid sourceId)
    {
        var knowledgeBases = await _repository.GetKnowledgeBasesForSourceAsync(sourceId);
        return knowledgeBases.Select(kb => new KnowledgeBaseDto(
            kb.Id,
            kb.Name,
            kb.Description,
            kb.CreatedAt,
            kb.UpdatedAt
        ));
    }

    private static SourceReadDto MapToSourceReadDto(Source source)
    {
        return new SourceReadDto(
            source.Id,
            source.Name,
            source.Type,
            source.Server,
            source.Instructions,
            source.ResponseFormat,
            source.CreatedAt,
            source.UpdatedAt
        );
    }
}
