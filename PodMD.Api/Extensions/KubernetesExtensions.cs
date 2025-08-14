using k8s;

namespace PodMD.Api.Extensions;

public static class KubernetesExtensions
{
    public static async Task<Stream> ReadNamespacedPodLogAsync(
        this Kubernetes client,
        string name,
        string namespaceParameter,
        string container = null,
        bool? follow = null,
        bool? insecureSkipTLSVerifyBackend = null,
        int? limitBytes = null,
        bool? pretty = null,
        bool? previous = null,
        DateTimeOffset? sinceTime = null,
        string stream = null,
        int? tailLines = null,
        bool? timestamps = null,
        CancellationToken cancellationToken = default)
    {
        int? sinceSeconds = null;
        if (sinceTime.HasValue)
        {
            var diff = DateTimeOffset.UtcNow - sinceTime.Value;
            sinceSeconds = (int)Math.Max(diff.TotalSeconds, 0);
        }

        var result = await client.CoreV1.ReadNamespacedPodLogWithHttpMessagesAsync(
            name,
            namespaceParameter,
            container,
            follow,
            insecureSkipTLSVerifyBackend,
            limitBytes,
            pretty,
            previous,
            sinceSeconds,
            stream,
            tailLines,
            timestamps,
            null,
            cancellationToken);
        result.Request.Dispose();
        return result.Body;
    }
}