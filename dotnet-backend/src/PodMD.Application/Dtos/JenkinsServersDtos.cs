using System.ComponentModel.DataAnnotations;

namespace PodMD.Application.Dtos;

public record CreateJenkinsServersRequest : SourceCreateDto
{
    public CreateJenkinsServersRequest(
    string Name,
    string Server,
    string? Instructions,
    string? ResponseFormat,
    [Required][StringLength(100)] string Username,
    [Required] string ApiToken)
        : base(Name, Server, Instructions, ResponseFormat)
    {
        this.Username = Username;
        this.ApiToken = ApiToken;
    }

    public string Username { get; init; } = string.Empty;
    public string ApiToken { get; init; } = string.Empty; // Before encryption
}

public record UpdateJenkinsServersRequest(
    string? Name,
    string? Server,
    string? Instructions,
    string? ResponseFormat,
    string? Username,
    string? ApiToken
);

public record JenkinsServersResponse : SourceReadDto
{
    public JenkinsServersResponse(
        SourceReadDto source,
        string Username)
        : base(
            source.Id,
            source.Name,
            source.Type,
            source.Server,
            source.Instructions,
            source.ResponseFormat,
            source.CreatedAt,
            source.UpdatedAt)
    {
        this.Username = Username;
    }

    public string Username { get; init; }
}
