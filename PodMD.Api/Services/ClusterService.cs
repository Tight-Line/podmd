using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using DotNext;
using k8s;
using Microsoft.EntityFrameworkCore;
using PodMD.Api.Extensions;
using PodMD.Api.Database;
using PodMD.Api.Models;

namespace PodMD.Api.Services;

public interface IClusterService
{
    Task<Result<Cluster>> CreateAsync(string host, string token, string? caCertPem);
    Task<Result<Cluster>> UpdateAsync(Guid guid, string host, string token, string? caCertPem);
    Task<Result<bool>> DeleteAsync(Guid guid);
    Task<Result<List<Cluster>>> GetAllAsync();
    Task<Cluster?> GetByGuidAsync(Guid guid);

    Task<Result<bool>> TestConnectionAsync(string host, string token, string? caCertPem);

    Task<Result<string>> GetLogsAsync(string host, string accessToken, string? caCertPem, string ns, string pod,
        int? tail,
        DateTimeOffset? sinceTime);

    Task<Result<string>> GetLogsAsync(Cluster cluster, string ns, string pod, int? tail, DateTimeOffset? sinceTime);
}

public class ClusterService(
    ILogger<ClusterService> logger,
    AppDbContext dbContext,
    IAuthenticatedApiKeyService authenticatedApiKeyService,
    IProtectionService protectionService) : IClusterService
{
    public async Task<Result<Cluster>> CreateAsync(string host, string token, string? caCertPem)
    {
        var cluster = new Cluster()
        {
            Host = host,
            AccessToken = protectionService.Protect(token),
            CaCertPem = caCertPem is not null ? protectionService.Protect(caCertPem) : null,
            Guid = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            ApiKeyId = authenticatedApiKeyService.ApiKeyId
        };

        dbContext.Clusters.Add(cluster);
        await dbContext.SaveChangesAsync();

        return cluster;
    }

    public async Task<Result<Cluster>> UpdateAsync(Guid guid, string host, string token, string? caCertPem)
    {
        var cluster = await dbContext.Clusters.FirstOrDefaultAsync(c =>
            c.Guid == guid && c.ApiKeyId == authenticatedApiKeyService.ApiKeyId);

        if (cluster is null)
            return Result.FromException<Cluster>(new KeyNotFoundException($"Cluster with Guid {guid} not found"));

        cluster.Host = host;
        cluster.AccessToken = protectionService.Protect(token);
        cluster.CaCertPem = caCertPem is not null ? protectionService.Protect(caCertPem) : null;

        dbContext.Clusters.Update(cluster);
        await dbContext.SaveChangesAsync();

        return cluster;
    }

    public async Task<Result<bool>> DeleteAsync(Guid guid)
    {
        var cluster = await dbContext.Clusters.FirstOrDefaultAsync(c =>
            c.Guid == guid && c.ApiKeyId == authenticatedApiKeyService.ApiKeyId);

        if (cluster is null)
            return Result.FromException<bool>(new KeyNotFoundException($"Cluster with Guid {guid} not found"));

        dbContext.Clusters.Remove(cluster);
        await dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<Result<List<Cluster>>> GetAllAsync()
    {
        try
        {
            var clusters = await dbContext.Clusters
                .Where(c => c.ApiKeyId == authenticatedApiKeyService.ApiKeyId)
                .ToListAsync();

            return Result.FromValue(clusters);
        }
        catch (Exception ex)
        {
            return Result.FromException<List<Cluster>>(ex);
        }
    }

    public async Task<Cluster?> GetByGuidAsync(Guid guid)
    {
        return await dbContext.Clusters
            .Include(c => c.KnowledgeBases)
            .FirstOrDefaultAsync(c => c.Guid == guid && c.ApiKeyId == authenticatedApiKeyService.ApiKeyId);
    }

    public async Task<Result<bool>> TestConnectionAsync(string host, string token, string? caCertPem)
    {
        try
        {
            var config = new KubernetesClientConfiguration
            {
                Host = host,
                AccessToken = token,
                SslCaCerts = caCertPem is not null
                    ? new X509Certificate2Collection(
                        X509CertificateLoader.LoadCertificate(Encoding.UTF8.GetBytes(caCertPem)))
                    : null
            };

            using var client = new Kubernetes(config);
            await client.ListNamespaceAsync();

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task<Result<string>> GetLogsAsync(Cluster cluster, string ns,
        string pod,
        int? tail = 100,
        DateTimeOffset? sinceTime = null)
    {
        string? accessToken;
        string? caCertPem = null;
        try
        {
            accessToken = protectionService.Unprotect(cluster.AccessToken);
            if (cluster.CaCertPem is not null) caCertPem = protectionService.Unprotect(cluster.CaCertPem);
        }
        catch (CryptographicException ex)
        {
            logger.LogError(ex, "Failed to unprotect access token");
            return new Result<string>(new Exception("Failed to unprotect access token"));
        }

        return await GetLogsAsync(cluster.Host, accessToken, caCertPem, ns, pod, tail, sinceTime);
    }

    public async Task<Result<string>> GetLogsAsync(string host, string accessToken, string? caCertPem, string ns,
        string pod,
        int? tail = 100,
        DateTimeOffset? sinceTime = null)
    {
        var config = new KubernetesClientConfiguration
        {
            Host = host,
            AccessToken = accessToken,
            SslCaCerts = caCertPem is not null
                ? new X509Certificate2Collection(
                    X509CertificateLoader.LoadCertificate(Encoding.UTF8.GetBytes(caCertPem)))
                : null
        };

        using var client = new Kubernetes(config);

        var logStream = await client.ReadNamespacedPodLogAsync(pod, ns, tailLines: tail, sinceTime: sinceTime);
        using var reader = new StreamReader(logStream, Encoding.UTF8);
        var logs = await reader.ReadToEndAsync();

        return Result.FromValue(logs);
    }
}