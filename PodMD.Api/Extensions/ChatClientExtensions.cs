using System.Text.Json;
using DotNext;
using OpenAI.Chat;

namespace PodMD.Api.Extensions;

public static class ChatClientExtensions
{
    public static async Task<Result<T>> TryCompleteChatAsync<T>(this ChatClient client,
        IEnumerable<ChatMessage> messages, ChatCompletionOptions? options = null)
    {
        var completion = await client.CompleteChatAsync(messages, options);
        var jsonResponse = completion.Value.Content[0].Text;

        try
        {
            var value = JsonSerializer.Deserialize<T>(jsonResponse);

            return value is null
                ? Result.FromException<T>(new JsonException("Deserialized value is null"))
                : Result.FromValue(value);
        }
        catch (Exception ex)
        {
            return Result.FromException<T>(ex);
        }
    }
}