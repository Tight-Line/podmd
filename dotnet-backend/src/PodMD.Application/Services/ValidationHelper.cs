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
}
