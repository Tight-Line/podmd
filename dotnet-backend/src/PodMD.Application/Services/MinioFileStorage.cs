using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using PodMD.Application.Configuration;
using PodMD.Domain.Interfaces;
using System.Text.RegularExpressions;

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

            // Check if the response is actually an XML error response
            if (IsErrorXmlResponse(memoryStream))
            {
                var errorMessage = ExtractErrorMessage(memoryStream);
                _logger.LogError("MinIO returned error XML for {StorageKey}: {ErrorMessage}", storageKey, errorMessage);

                if (errorMessage.Contains("AccessDenied", StringComparison.OrdinalIgnoreCase) ||
                    errorMessage.Contains("403", StringComparison.OrdinalIgnoreCase))
                {
                    throw new UnauthorizedAccessException($"Access denied downloading file: {storageKey}");
                }
                else if (errorMessage.Contains("NoSuchKey", StringComparison.OrdinalIgnoreCase) ||
                         errorMessage.Contains("404", StringComparison.OrdinalIgnoreCase))
                {
                    throw new FileNotFoundException($"File not found: {storageKey}");
                }
                else
                {
                    throw new InvalidOperationException($"MinIO error: {errorMessage}");
                }
            }

            _logger.LogInformation("File downloaded from MinIO: {StorageKey}", storageKey);
            return memoryStream;
        }
        catch (Minio.Exceptions.ObjectNotFoundException)
        {
            _logger.LogWarning("File not found in MinIO: {StorageKey}", storageKey);
            throw new FileNotFoundException($"File not found: {storageKey}");
        }
        catch (Exception ex) when (!(ex is FileNotFoundException || ex is UnauthorizedAccessException))
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

    private bool IsErrorXmlResponse(Stream memoryStream)
    {
        try
        {
            if (memoryStream.Length == 0)
                return false;

            memoryStream.Position = 0;
            using var reader = new StreamReader(memoryStream, leaveOpen: true);
            var content = reader.ReadToEnd();
            memoryStream.Position = 0; // Reset for potential reuse

            // Check if it starts with XML declaration and contains error elements
            return content.TrimStart().StartsWith("<?xml", StringComparison.OrdinalIgnoreCase) &&
                   (content.Contains("<Error>", StringComparison.OrdinalIgnoreCase) ||
                    content.Contains("AccessDenied", StringComparison.OrdinalIgnoreCase) ||
                    content.Contains("403", StringComparison.Ordinal) ||
                    content.Contains("404", StringComparison.Ordinal) ||
                    content.Contains("NoSuchKey", StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            // If we can't read it, assume it's not an error
            memoryStream.Position = 0;
            return false;
        }
    }

    private string ExtractErrorMessage(Stream memoryStream)
    {
        try
        {
            memoryStream.Position = 0;
            using var reader = new StreamReader(memoryStream, leaveOpen: true);
            var content = reader.ReadToEnd();

            // Try to extract the Code and Message from the XML
            var codeMatch = Regex.Match(content, @"<Code>(.*?)</Code>", RegexOptions.IgnoreCase);
            var messageMatch = Regex.Match(content, @"<Message>(.*?)</Message>", RegexOptions.IgnoreCase);

            var code = codeMatch.Success ? codeMatch.Groups[1].Value : "Unknown";
            var message = messageMatch.Success ? messageMatch.Groups[1].Value : content.Length > 200 ? content.Substring(0, 200) + "..." : content;

            return $"Code: {code}, Message: {message}";
        }
        catch (Exception ex)
        {
            return $"Failed to parse error response: {ex.Message}";
        }
    }
}
