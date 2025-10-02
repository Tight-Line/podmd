using PodMD.Application.Dtos;
using PodMD.Application.Interfaces;
using PodMD.Domain.Entities;

namespace PodMD.Application.Services;

public class JenkinsServersService : IJenkinsServersService
{
    private readonly IJenkinsServersRepository _repository;
    private readonly EncryptionService _encryptionService;

    public JenkinsServersService(
        IJenkinsServersRepository repository,
        EncryptionService encryptionService)
    {
        _repository = repository;
        _encryptionService = encryptionService;
    }

    public async Task<JenkinsServersResponse> CreateAsync(CreateJenkinsServersRequest request)
    {
        // Validate HTTPS URL
        ValidationHelper.ValidateHttpsUrl(request.Server);

        // Encrypt the API token
        var encryptedToken = _encryptionService.Encrypt(request.ApiToken);

        var server = new JenkinsServers
        {
            Type = "Jenkins", // Explicit type for TPT inheritance
            Name = request.Name,
            Server = request.Server,
            Username = request.Username,
            ApiTokenEnc = encryptedToken,
            Instructions = request.Instructions,
            ResponseFormat = request.ResponseFormat,
            KeyVersion = 1, // Current key version
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdServer = await _repository.AddAsync(server);

        return MapToResponse(createdServer);
    }

    public async Task<JenkinsServersResponse?> GetByIdAsync(Guid id)
    {
        var server = await _repository.GetByIdAsync(id);
        return server != null ? MapToResponse(server) : null;
    }

    public async Task<IEnumerable<JenkinsServersResponse>> GetAllAsync()
    {
        var servers = await _repository.GetAllAsync();
        return servers.Select(MapToResponse);
    }

    public async Task<JenkinsServersResponse> UpdateAsync(Guid id, UpdateJenkinsServersRequest request)
    {
        var server = await _repository.GetByIdAsync(id);
        if (server == null)
        {
            throw new KeyNotFoundException($"Jenkins server with ID '{id}' not found.");
        }

        // Validate HTTPS URL if server is being changed
        if (!string.IsNullOrEmpty(request.Server))
        {
            ValidationHelper.ValidateHttpsUrl(request.Server);
            server.Server = request.Server;
        }

        // Encrypt new API token if provided
        if (!string.IsNullOrEmpty(request.ApiToken))
        {
            server.ApiTokenEnc = _encryptionService.Encrypt(request.ApiToken);
        }

        // Update other fields
        if (!string.IsNullOrEmpty(request.Name))
        {
            server.Name = request.Name;
        }

        if (!string.IsNullOrEmpty(request.Username))
        {
            server.Username = request.Username;
        }

        if (request.Instructions != null)
        {
            server.Instructions = request.Instructions;
        }

        if (request.ResponseFormat != null)
        {
            server.ResponseFormat = request.ResponseFormat;
        }

        server.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(server);

        return MapToResponse(server);
    }

    public async Task DeleteAsync(Guid id)
    {
        var server = await _repository.GetByIdAsync(id);
        if (server == null)
        {
            throw new KeyNotFoundException($"Jenkins server with ID '{id}' not found.");
        }

        await _repository.DeleteAsync(server);
    }

    private static JenkinsServersResponse MapToResponse(JenkinsServers server)
    {
        var sourceDto = new SourceReadDto(
            server.Id,
            server.Name,
            server.Type,
            server.Server,
            server.Instructions,
            server.ResponseFormat,
            server.CreatedAt,
            server.UpdatedAt
        );

        return new JenkinsServersResponse(sourceDto, server.Username);
    }
}
