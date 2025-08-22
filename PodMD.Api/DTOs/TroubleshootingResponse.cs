namespace PodMD.Api.DTOs;

using System.Collections.Generic;
using System.Text.Json.Serialization;

public class TroubleshootingResponse
{
    [JsonPropertyName("errors")] public List<ErrorGroup> Errors { get; set; } = new();

    public class ErrorGroup
    {
        [JsonPropertyName("general_message")] public string GeneralMessage { get; set; } = string.Empty;

        [JsonPropertyName("occurrences")] public List<string> Occurrences { get; set; } = new();

        [JsonPropertyName("solutions")] public List<Solution> Solutions { get; set; } = new();

        public class Solution
        {
            [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;

            [JsonPropertyName("steps")] public List<SolutionStep> Steps { get; set; } = new();

            public class SolutionStep
            {
                [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;

                [JsonPropertyName("explanation")] public string Explanation { get; set; } = string.Empty;

                [JsonPropertyName("command")] public string? Command { get; set; }
            }
        }
    }
}