using DotNext;
using OpenAI;
using OpenAI.Chat;
using PodMD.Api.DTOs;
using PodMD.Api.DTOs.Schemas;
using PodMD.Api.Extensions;

namespace PodMD.Api.Services;

public interface IChatService
{
    Task<Result<TroubleshootingResponse>> AskAsync(string logs);
}

public class ChatService : IChatService
{
    private readonly ChatClient _chatClient;

    public ChatService(OpenAIClient openAiClient, ChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<Result<TroubleshootingResponse>> AskAsync(string logs)
    {
        List<ChatMessage> messages =
        [
            new SystemChatMessage(Prompts.TroubleshootingPrompt),
            new UserChatMessage(logs)
        ];

        var options = new ChatCompletionOptions()
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                "troubleshooting_response",
                BinaryData.FromString(TroubleshootingResponseSchema.Value),
                jsonSchemaIsStrict: true)
        };

        return await _chatClient.TryCompleteChatAsync<TroubleshootingResponse>(messages, options);
    }
}