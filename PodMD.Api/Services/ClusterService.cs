using System.Security.Cryptography;
using System.Text;
using DotNext;
using k8s;
using Microsoft.EntityFrameworkCore;
using PodMD.Api.Extensions;
using PodMD.Api.Database;
using PodMD.Api.DTOs;
using PodMD.Api.Models;

namespace PodMD.Api.Services;

public interface IClusterService
{
    Task<Result<Cluster>> CreateAsync(string host, string token);
    Task<Result<Cluster>> UpdateAsync(Guid guid, string host, string token);
    Task<Result<bool>> DeleteAsync(Guid guid);
    Task<Cluster?> GetByGuid(Guid guid);

    Task<Result<TroubleshootingResponse>> AnalyzeLogsAsync(Cluster cluster, string ns, string pod,
        int? tail,
        DateTimeOffset? sinceTime);
}

public class ClusterService(
    ILogger<ClusterService> logger,
    AppDbContext dbContext,
    IChatService chatService,
    IRagService ragService,
    IProtectionService protectionService) : IClusterService
{
    public async Task<Result<Cluster>> CreateAsync(string host, string token)
    {
        var cluster = new Cluster()
        {
            Host = host,
            AccessToken = protectionService.Protect(token),
            Guid = Guid.NewGuid()
        };

        dbContext.Clusters.Add(cluster);
        await dbContext.SaveChangesAsync();

        return cluster;
    }

    public async Task<Result<Cluster>> UpdateAsync(Guid guid, string host, string token)
    {
        var cluster = await dbContext.Clusters.FirstOrDefaultAsync(c => c.Guid == guid);

        if (cluster is null)
            return Result.FromException<Cluster>(new KeyNotFoundException($"Cluster with Guid {guid} not found"));

        cluster.Host = host;
        cluster.AccessToken = protectionService.Protect(token);

        dbContext.Clusters.Update(cluster);
        await dbContext.SaveChangesAsync();

        return cluster;
    }

    public async Task<Result<bool>> DeleteAsync(Guid guid)
    {
        var cluster = await dbContext.Clusters.FirstOrDefaultAsync(c => c.Guid == guid);

        if (cluster is null)
            return Result.FromException<bool>(new KeyNotFoundException($"Cluster with Guid {guid} not found"));

        dbContext.Clusters.Remove(cluster);
        await dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<Cluster?> GetByGuid(Guid guid)
    {
        return await dbContext.Clusters.FirstOrDefaultAsync(c => c.Guid == guid);
    }

    public async Task<Result<TroubleshootingResponse>> AnalyzeLogsAsync(Cluster cluster, string ns,
        string pod,
        int? tail = 100,
        DateTimeOffset? sinceTime = null)
    {
        string? accessToken = null;
        try
        {
            accessToken = protectionService.Unprotect(cluster.AccessToken);
        }
        catch (CryptographicException ex)
        {
            logger.LogError(ex, "Failed to unprotect access token");
            return new Result<TroubleshootingResponse>(new Exception("Failed to unprotect access token"));
        }

        var config = new KubernetesClientConfiguration
        {
            Host = cluster.Host,
            AccessToken = accessToken,
            SkipTlsVerify = true
        };

        using var client = new Kubernetes(config);

        var logStream = await client.ReadNamespacedPodLogAsync(pod, ns, tailLines: tail, sinceTime: sinceTime);
        using var reader = new StreamReader(logStream, Encoding.UTF8);
        var logs = await reader.ReadToEndAsync();

        if (cluster.OpenAIAssistantId is not null)
            return await ragService.AskAsync(cluster.OpenAIAssistantId, logs);

        return await chatService.AskAsync(logs);
    }
}