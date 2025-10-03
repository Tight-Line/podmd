namespace PodMD.Application.Services;

public static class ValidationHelper
{
    public static void ValidateHttpsUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException("Server must be a valid HTTPS URL.");
        }
    }

    public static void ValidateFileType(string contentType, string allowedTypesCsv)
    {
        var allowedTypes = allowedTypesCsv.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        if (allowedTypes.Contains(contentType))
        {
            return;
        }

        var supportedTypes = string.Join(", ", allowedTypes);
        throw new InvalidOperationException($"File type '{contentType}' is not supported. Supported types: {supportedTypes}");
    }

    public static void ValidateFileSize(long fileSize, long maxSizeBytes)
    {
        if (fileSize > maxSizeBytes)
        {
            var maxSizeMB = maxSizeBytes / (1024 * 1024L);
            throw new InvalidOperationException($"File size exceeds the maximum limit of {maxSizeMB} MB.");
        }
    }
}
