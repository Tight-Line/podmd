using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using PodMD.Application.Configuration;
using PodMD.Domain.Interfaces;

namespace PodMD.Application.Services;

public class MinioFileStorage : IFileStorage
{
    private readonly IMinioClient _minioClient;
    private readonly MinioSettings _settings;
    private readonly ILogger<MinioFileStorage> _logger;

    public MinioFileStorage(IOptions<MinioSettings> settings, ILogger<MinioFileStorage> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        _minioClient = new MinioClient()
            .WithEndpoint(_settings.Endpoint)
            .WithCredentials(_settings.AccessKey, _settings.SecretKey)
            .WithSSL(false) // MinIO in docker-compose doesn't use SSL
            .Build();
    }

    public async Task<string> UploadAsync(string fileName, string contentType, Stream fileStream, string storageKey)
    {
        try
        {
            // Ensure bucket exists
            await EnsureBucketExistsAsync();

            // Upload the file
            var args = new PutObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(storageKey)
                .WithStreamData(fileStream)
                .WithObjectSize(fileStream.Length)
                .WithContentType(contentType);

            await _minioClient.PutObjectAsync(args);

            _logger.LogInformation("File uploaded to MinIO: {StorageKey}", storageKey);
            return storageKey;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to MinIO: {StorageKey}", storageKey);
            throw new InvalidOperationException($"Failed to upload file: {ex.Message}", ex);
        }
    }

    public async Task<Stream> DownloadAsync(string storageKey)
    {
        try
        {
            var memoryStream = new MemoryStream();

            var args = new GetObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(storageKey)
                .WithCallbackStream(stream =>
                {
                    stream.CopyTo(memoryStream);
                });

            await _minioClient.GetObjectAsync(args);

            memoryStream.Position = 0;
            _logger.LogInformation("File downloaded from MinIO: {StorageKey}", storageKey);
            return memoryStream;
        }
        catch (Minio.Exceptions.ObjectNotFoundException)
        {
            _logger.LogWarning("File not found in MinIO: {StorageKey}", storageKey);
            throw new FileNotFoundException($"File not found: {storageKey}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file from MinIO: {StorageKey}", storageKey);
            throw new InvalidOperationException($"Failed to download file: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteAsync(string storageKey)
    {
        try
        {
            var args = new RemoveObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(storageKey);

            await _minioClient.RemoveObjectAsync(args);

            _logger.LogInformation("File deleted from MinIO: {StorageKey}", storageKey);
            return true;
        }
        catch (Minio.Exceptions.ObjectNotFoundException)
        {
            _logger.LogWarning("File not found for deletion in MinIO: {StorageKey}", storageKey);
            return false; // File is already gone
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from MinIO: {StorageKey}", storageKey);
            throw new InvalidOperationException($"Failed to delete file: {ex.Message}", ex);
        }
    }

    public async Task<bool> ExistsAsync(string storageKey)
    {
        try
        {
            var args = new StatObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(storageKey);

            await _minioClient.StatObjectAsync(args);

            _logger.LogDebug("File exists in MinIO: {StorageKey}", storageKey);
            return true;
        }
        catch (Minio.Exceptions.ObjectNotFoundException)
        {
            _logger.LogDebug("File does not exist in MinIO: {StorageKey}", storageKey);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence in MinIO: {StorageKey}", storageKey);
            throw new InvalidOperationException($"Failed to check file existence: {ex.Message}", ex);
        }
    }

    private async Task EnsureBucketExistsAsync()
    {
        try
        {
            var args = new BucketExistsArgs()
                .WithBucket(_settings.BucketName);

            bool found = await _minioClient.BucketExistsAsync(args);

            if (!found)
            {
                var makeBucketArgs = new MakeBucketArgs()
                    .WithBucket(_settings.BucketName);

                await _minioClient.MakeBucketAsync(makeBucketArgs);
                _logger.LogInformation("MinIO bucket created: {BucketName}", _settings.BucketName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring MinIO bucket exists: {BucketName}", _settings.BucketName);
            throw new InvalidOperationException($"Failed to ensure bucket exists: {ex.Message}", ex);
        }
    }
}
