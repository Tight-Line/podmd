using Microsoft.Extensions.Options;
using PodMD.Application.Configuration;
using PodMD.Application.Dtos;
using PodMD.Application.Interfaces;
using PodMD.Domain.Entities;
using PodMD.Domain.Interfaces;

namespace PodMD.Application.Services;

public class KnowledgeFileService : IKnowledgeFileService
{
    private readonly IKnowledgeFileRepository _repository;
    private readonly IFileStorage _fileStorage;
    private readonly MinioSettings _minioSettings;
    private readonly ILogger<KnowledgeFileService> _logger;

    public KnowledgeFileService(
        IKnowledgeFileRepository repository,
        IFileStorage fileStorage,
        IOptions<MinioSettings> minioSettings,
        ILogger<KnowledgeFileService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
        _minioSettings = minioSettings.Value;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<KnowledgeFileDto>> UploadFilesAsync(IEnumerable<CreateKnowledgeFileDto> uploadRequests)
    {
        var results = new List<KnowledgeFileDto>();

        foreach (var request in uploadRequests)
        {
            // Validate file
            ValidationHelper.ValidateFileType(request.ContentType, _minioSettings.AllowedMimeTypes);
            ValidationHelper.ValidateFileSize(request.FileSize, _minioSettings.MaxFileSizeMb * 1024 * 1024L);

            // Check for duplicate filename
            var existingFile = await _repository.GetByKnowledgeBaseIdAndFileNameAsync(request.KnowledgeBaseId, request.FileName);
            if (existingFile != null)
            {
                throw new InvalidOperationException($"File with name '{request.FileName}' already exists in knowledge base '{request.KnowledgeBaseId}'.");
            }

            // Generate storage key
            var storageKey = GenerateStorageKey(request.KnowledgeBaseId, request.FileName);

            try
            {
                // Upload to MinIO
                await _fileStorage.UploadAsync(request.FileName, request.ContentType, request.FileStream, storageKey);

                // Create entity
                var knowledgeFile = new KnowledgeFile
                {
                    Id = Guid.NewGuid(),
                    KnowledgeBaseId = request.KnowledgeBaseId,
                    FileName = request.FileName,
                    StorageKey = storageKey,
                    ContentType = request.ContentType,
                    FileSize = request.FileSize,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Save to database
                await _repository.CreateAsync(knowledgeFile);

                var result = new KnowledgeFileDto(
                    knowledgeFile.Id,
                    knowledgeFile.KnowledgeBaseId,
                    knowledgeFile.FileName,
                    knowledgeFile.ContentType,
                    knowledgeFile.FileSize,
                    knowledgeFile.CreatedAt,
                    knowledgeFile.UpdatedAt);

                results.Add(result);
                _logger.LogInformation("File uploaded successfully: {FileName} (ID: {FileId})", request.FileName, knowledgeFile.Id);
            }
            catch
            {
                // Cleanup failed upload from MinIO
                try
                {
                    await _fileStorage.DeleteAsync(storageKey);
                }
                catch { /* Ignore cleanup errors */ }

                throw;
            }
        }

        return results;
    }

    public async Task<IEnumerable<KnowledgeFileDto>> GetByKnowledgeBaseIdAsync(Guid knowledgeBaseId)
    {
        var files = await _repository.GetByKnowledgeBaseIdAsync(knowledgeBaseId);

        return files.Select(kf => new KnowledgeFileDto(
            kf.Id,
            kf.KnowledgeBaseId,
            kf.FileName,
            kf.ContentType,
            kf.FileSize,
            kf.CreatedAt,
            kf.UpdatedAt));
    }

    public async Task<KnowledgeFileDto?> GetByIdAsync(Guid id)
    {
        var knowledgeFile = await _repository.GetByIdAsync(id);

        if (knowledgeFile == null)
        {
            return null;
        }

        return new KnowledgeFileDto(
            knowledgeFile.Id,
            knowledgeFile.KnowledgeBaseId,
            knowledgeFile.FileName,
            knowledgeFile.ContentType,
            knowledgeFile.FileSize,
            knowledgeFile.CreatedAt,
            knowledgeFile.UpdatedAt);
    }

    public async Task<KnowledgeFileDto> ReplaceFileAsync(Guid fileId, UpdateKnowledgeFileDto request)
    {
        // Get existing file
        var existingFile = await _repository.GetByIdAsync(fileId);
        if (existingFile == null)
        {
            throw new KeyNotFoundException($"Knowledge file with ID '{fileId}' not found.");
        }

        // Validate new file
        ValidationHelper.ValidateFileType(request.ContentType, _minioSettings.AllowedMimeTypes);
        ValidationHelper.ValidateFileSize(request.FileSize, _minioSettings.MaxFileSizeMb * 1024 * 1024L);

        // Check for filename conflict if name changed
        string newFileName = request.FileName ?? existingFile.FileName;
        if (request.FileName != null && request.FileName != existingFile.FileName)
        {
            var conflictingFile = await _repository.GetByKnowledgeBaseIdAndFileNameAsync(existingFile.KnowledgeBaseId, request.FileName);
            if (conflictingFile != null && conflictingFile.Id != fileId)
            {
                throw new InvalidOperationException($"File with name '{request.FileName}' already exists in knowledge base '{existingFile.KnowledgeBaseId}'.");
            }
        }

        // Generate new storage key
        var newStorageKey = GenerateStorageKey(existingFile.KnowledgeBaseId, newFileName);

        try
        {
            // Upload new file
            await _fileStorage.UploadAsync(newFileName, request.ContentType, request.FileStream, newStorageKey);

            try
            {
                // Delete old file
                await _fileStorage.DeleteAsync(existingFile.StorageKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete old file from storage: {StorageKey}", existingFile.StorageKey);
                // Continue with replacement even if old file deletion fails
            }

            // Update entity
            existingFile.FileName = newFileName;
            existingFile.StorageKey = newStorageKey;
            existingFile.ContentType = request.ContentType;
            existingFile.FileSize = request.FileSize;
            existingFile.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(existingFile);

            var result = new KnowledgeFileDto(
                existingFile.Id,
                existingFile.KnowledgeBaseId,
                existingFile.FileName,
                existingFile.ContentType,
                existingFile.FileSize,
                existingFile.CreatedAt,
                existingFile.UpdatedAt);

            _logger.LogInformation("File replaced successfully: {FileName} (ID: {FileId})", newFileName, fileId);
            return result;
        }
        catch
        {
            // Cleanup failed upload
            try
            {
                await _fileStorage.DeleteAsync(newStorageKey);
            }
            catch { /* Ignore cleanup errors */ }

            throw;
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        // Get file first
        var knowledgeFile = await _repository.GetByIdAsync(id);
        if (knowledgeFile == null)
        {
            throw new KeyNotFoundException($"Knowledge file with ID '{id}' not found.");
        }

        // Get storage key for cleanup
        string storageKey = knowledgeFile.StorageKey;

        // Hard delete from database
        await _repository.DeleteAsync(id);

        // Delete from storage (hard delete is irreversible, so be cautious)
        try
        {
            await _fileStorage.DeleteAsync(storageKey);
            _logger.LogInformation("File deleted successfully: {FileName} (ID: {FileId})", knowledgeFile.FileName, id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file from storage after database deletion: {StorageKey}", storageKey);
            // Database record is already removed, this is serious - log as error
            throw new InvalidOperationException($"File deleted from database but storage cleanup failed: {storageKey}", ex);
        }
    }

    private static string GenerateStorageKey(Guid knowledgeBaseId, string fileName)
    {
        var fileGuid = Guid.NewGuid();
        return $"knowledgebase-{knowledgeBaseId}/file-{fileGuid}";
    }
}
