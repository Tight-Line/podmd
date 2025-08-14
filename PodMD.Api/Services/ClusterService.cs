using System.Security.Cryptography;
using System.Text;
using DotNext;
using k8s;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using PodMD.Api.Extensions;
using PodMD.Api.Database;
using PodMD.Api.DTOs;
using PodMD.Api.Models;

namespace PodMD.Api.Services;

public class ClusterService(
    ILogger<ClusterService> logger,
    AppDbContext dbContext,
    OpenAIService openAiService,
    IProtectionService protectionService) : IClusterService
{
    public async Task<Result<Cluster>> Create(string host, string token)
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

    public async Task<Cluster?> GetByGuid(Guid guid)
    {
        return await dbContext.Clusters.FirstOrDefaultAsync(c => c.Guid == guid);
    }

    public async Task<Result<TroubleshootingResponse>> AnalyzeLogsAsync(string host, string accessToken, string ns,
        string pod,
        int? tail = 100,
        DateTimeOffset? sinceTime = null)
    {
        try
        {
            accessToken = protectionService.Unprotect(accessToken);
        }
        catch (CryptographicException ex)
        {
            logger.LogError(ex, "Failed to unprotect access token");
            return new Result<TroubleshootingResponse>(new Exception("Failed to unprotect access token"));
        }

        var config = new KubernetesClientConfiguration
        {
            Host = host,
            AccessToken = accessToken,
            SkipTlsVerify = true
        };

        using var client = new Kubernetes(config);

        var logStream = await client.ReadNamespacedPodLogAsync(pod, ns, tailLines: tail, sinceTime: sinceTime);
        using var reader = new StreamReader(logStream, Encoding.UTF8);
        var logs = await reader.ReadToEndAsync();

        return await openAiService.AskAsync(logs);
    }
}

public interface IClusterService
{
    Task<Result<Cluster>> Create(string host, string token);
    Task<Cluster?> GetByGuid(Guid guid);

    Task<Result<TroubleshootingResponse>> AnalyzeLogsAsync(string host, string accessToken, string ns, string pod,
        int? tail,
        DateTimeOffset? sinceTime);
}