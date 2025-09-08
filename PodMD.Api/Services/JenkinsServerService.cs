using System.Net.Http.Headers;
using System.Text;
using DotNext;
using Microsoft.EntityFrameworkCore;
using PodMD.Api.Database;
using PodMD.Api.Models;

namespace PodMD.Api.Services;

public interface IJenkinsServerService
{
    Task<Result<JenkinsServer>> CreateAsync(string name, string host, string username, string token);

    Task<Result<JenkinsServer>> UpdateAsync(JenkinsServer jenkinsServer, string name, string host, string username,
        string token,
        CancellationToken ct = default);

    Task<Result<bool>> DeleteAsync(Guid guid);
    Task<Result<List<JenkinsServer>>> GetAllAsync();
    Task<JenkinsServer?> GetByGuidAsync(Guid guid);

    Task<Result<bool>> TestConnectionAsync(string host, string username, string apiToken);

    Task<Result<string>> GetLogsAsync(JenkinsServer jenkinsServer, string jobName, int buildNumber,
        CancellationToken ct = default);
}

public class JenkinsServerService(
    IHttpClientFactory httpClientFactory,
    AppDbContext dbContext,
    IAuthenticatedApiKeyService authenticatedApiKeyService,
    IProtectionService protectionService) : IJenkinsServerService
{
    public async Task<Result<JenkinsServer>> CreateAsync(string name, string host, string username, string token)
    {
        var jenkinsServer = new JenkinsServer()
        {
            Name = name,
            Host = host,
            Username = username,
            ApiToken = protectionService.Protect(token),
            Guid = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            ApiKeyId = authenticatedApiKeyService.ApiKeyId
        };

        dbContext.JenkinsServers.Add(jenkinsServer);
        await dbContext.SaveChangesAsync();

        return jenkinsServer;
    }

    public async Task<Result<JenkinsServer>> UpdateAsync(JenkinsServer jenkinsServer, string name, string host,
        string username,
        string token, CancellationToken ct = default)
    {
        jenkinsServer.Name = name;
        jenkinsServer.Host = host;
        jenkinsServer.Username = username;
        jenkinsServer.ApiToken = protectionService.Protect(token);

        await dbContext.SaveChangesAsync(ct);

        return jenkinsServer;
    }

    public async Task<Result<bool>> DeleteAsync(Guid guid)
    {
        var jenkinsServer = await dbContext.JenkinsServers.FirstOrDefaultAsync(js =>
            js.Guid == guid && js.ApiKeyId == authenticatedApiKeyService.ApiKeyId);

        if (jenkinsServer is null)
            return Result.FromException<bool>(new KeyNotFoundException($"JenkinsServer with Guid {guid} not found"));

        dbContext.JenkinsServers.Remove(jenkinsServer);
        await dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<Result<List<JenkinsServer>>> GetAllAsync()
    {
        try
        {
            var jenkinsServers = await dbContext.JenkinsServers
                .Where(c => c.ApiKeyId == authenticatedApiKeyService.ApiKeyId)
                .ToListAsync();

            return Result.FromValue(jenkinsServers);
        }
        catch (Exception ex)
        {
            return Result.FromException<List<JenkinsServer>>(ex);
        }
    }

    public async Task<JenkinsServer?> GetByGuidAsync(Guid guid)
    {
        return await dbContext.JenkinsServers.Include(js => js.KnowledgeBases)
            .FirstOrDefaultAsync(js => js.Guid == guid && js.ApiKeyId == authenticatedApiKeyService.ApiKeyId);
    }

    public async Task<Result<bool>> TestConnectionAsync(string host, string username, string apiToken)
    {
        try
        {
            var httpClient = httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(host);

            var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{apiToken}"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

            using var response = await httpClient.GetAsync($"api/json");

            return Result.FromValue(response.IsSuccessStatusCode);
        }
        catch (Exception ex)
        {
            return Result.FromException<bool>(ex);
        }
    }

    public async Task<Result<string>> GetLogsAsync(JenkinsServer jenkinsServer, string jobName, int buildNumber,
        CancellationToken ct = default)
    {
        try
        {
            var httpClient = httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(jenkinsServer.Host);

            var apiToken = protectionService.Unprotect(jenkinsServer.ApiToken);
            var authToken =
                Convert.ToBase64String(
                    Encoding.ASCII.GetBytes(
                        $"{jenkinsServer.Username}:{apiToken}"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

            var encodedJobName = Uri.EscapeDataString(jobName);
            var url = $"job/{encodedJobName}/{buildNumber}/consoleText";

            using var response = await httpClient.GetAsync(url, ct);

            if (!response.IsSuccessStatusCode)
                return Result.FromException<string>(
                    new HttpRequestException($"Failed to fetch Jenkins logs. Status code: {response.StatusCode}"));

            var logs = await response.Content.ReadAsStringAsync(ct);
            return Result.FromValue(logs);
        }
        catch (Exception ex)
        {
            return Result.FromException<string>(ex);
        }
    }
}