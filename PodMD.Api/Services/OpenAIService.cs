using DotNext;
using OpenAI.Chat;
using PodMD.Api.DTOs;
using PodMD.Api.DTOs.Schemas;
using PodMD.Api.Extensions;

namespace PodMD.Api.Services;

public class OpenAIService
{
    private readonly ChatClient _chatClient;

    public OpenAIService(ChatClient chatClient)
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
                BinaryData.FromBytes(TroubleshootingResponseSchema.Value),
                jsonSchemaIsStrict: true)
        };

        return await _chatClient.TryCompleteChatAsync<TroubleshootingResponse>(messages, options);
    }
}