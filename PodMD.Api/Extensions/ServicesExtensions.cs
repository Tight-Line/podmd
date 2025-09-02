using System.ClientModel;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using PodMD.Api.Config;
using PodMD.Api.Services;

namespace PodMD.Api.Extensions;

public static class ServicesExtensions
{
    public static void RegisterAppServices(this IServiceCollection services)
    {
        services.AddSingleton<ChatClient>(provider =>
        {
            var openAIConfig = provider.GetRequiredService<IOptions<OpenAIConfig>>().Value;
            return new ChatClient(openAIConfig.Model, new ApiKeyCredential(openAIConfig.ApiKey),
                new OpenAIClientOptions() { Endpoint = new Uri(openAIConfig.BaseUrl) });
        });
        services.AddSingleton<OpenAIClient>(provider =>
        {
            var openAIConfig = provider.GetRequiredService<IOptions<OpenAIConfig>>().Value;
            return new OpenAIClient(new ApiKeyCredential(openAIConfig.ApiKey),
                new OpenAIClientOptions() { Endpoint = new Uri(openAIConfig.BaseUrl) });
        });
        services.AddSingleton<IProtectionService, AesProtectionService>(provider =>
        {
            var dataProtection = provider.GetRequiredService<IOptions<DataProtectionConfig>>().Value;
            return new AesProtectionService(dataProtection.TokenEncryptionKey);
        });

        services.AddSingleton<IHashingService, HashingService>();

        services.AddScoped<IApiKeyService, ApiKeyService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IOpenAIService, OpenAIService>();
        services.AddScoped<IConfigurationService, ConfigurationService>();
        services.AddScoped<IClusterService, ClusterService>();
        services.AddScoped<IKnowledgeBaseService, KnowledgeBaseService>();
        services.AddScoped<IResourceService, ResourceService>();
        services.AddScoped<IResponseRagService, ResponseRagService>();
        services.AddScoped<IAuthenticatedApiKeyService, AuthenticatedApiKeyService>();
    }
}