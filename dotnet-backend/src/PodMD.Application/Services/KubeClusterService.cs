using PodMD.Application.Dtos;
using PodMD.Application.Interfaces;
using PodMD.Domain.Entities;

namespace PodMD.Application.Services;

public class KubeClusterService : IKubeClusterService
{
    private readonly IKubeClusterRepository _repository;
    private readonly EncryptionService _encryptionService;

    public KubeClusterService(
        IKubeClusterRepository repository,
        EncryptionService encryptionService)
    {
        _repository = repository;
        _encryptionService = encryptionService;
    }

    public async Task<KubeClusterResponse> CreateAsync(CreateKubeClusterRequest request)
    {
        // Validate name uniqueness
        if (await _repository.NameExistsAsync(request.Name))
        {
            throw new InvalidOperationException($"A cluster with the name '{request.Name}' already exists.");
        }

        // Validate HTTPS URL
        if (!Uri.TryCreate(request.Server, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException("Server must be a valid HTTPS URL.");
        }

        // Encrypt the bearer token
        var encryptedToken = _encryptionService.Encrypt(request.BearerToken);

        var cluster = new KubeCluster
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Server = request.Server,
            BearerTokenEnc = encryptedToken,
            CertificateAuthorityPem = request.CertificateAuthorityPem,
            InsecureSkipTlsVerify = request.InsecureSkipTlsVerify,
            DefaultNamespace = request.DefaultNamespace,
            KeyVersion = 1, // Current key version
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdCluster = await _repository.AddAsync(cluster);

        return MapToResponse(createdCluster);
    }

    public async Task<KubeClusterResponse?> GetByIdAsync(Guid id)
    {
        var cluster = await _repository.GetByIdAsync(id);
        return cluster != null ? MapToResponse(cluster) : null;
    }

    public async Task<IEnumerable<KubeClusterResponse>> GetAllAsync()
    {
        var clusters = await _repository.GetAllAsync();
        return clusters.Select(MapToResponse);
    }

    public async Task<KubeClusterResponse> UpdateAsync(Guid id, UpdateKubeClusterRequest request)
    {
        var cluster = await _repository.GetByIdAsync(id);
        if (cluster == null)
        {
            throw new KeyNotFoundException($"Cluster with ID '{id}' not found.");
        }

        // Validate name uniqueness if name is being changed
        if (!string.IsNullOrEmpty(request.Name) && request.Name != cluster.Name)
        {
            if (await _repository.NameExistsAsync(request.Name, id))
            {
                throw new InvalidOperationException($"A cluster with the name '{request.Name}' already exists.");
            }
            cluster.Name = request.Name;
        }

        // Validate HTTPS URL if server is being changed
        if (!string.IsNullOrEmpty(request.Server))
        {
            if (!Uri.TryCreate(request.Server, UriKind.Absolute, out var uri) ||
                uri.Scheme != Uri.UriSchemeHttps)
            {
                throw new InvalidOperationException("Server must be a valid HTTPS URL.");
            }
            cluster.Server = request.Server;
        }

        // Encrypt new bearer token if provided
        if (!string.IsNullOrEmpty(request.BearerToken))
        {
            cluster.BearerTokenEnc = _encryptionService.Encrypt(request.BearerToken);
        }

        // Update other fields
        if (request.CertificateAuthorityPem != null)
        {
            cluster.CertificateAuthorityPem = request.CertificateAuthorityPem;
        }

        if (request.InsecureSkipTlsVerify.HasValue)
        {
            cluster.InsecureSkipTlsVerify = request.InsecureSkipTlsVerify.Value;
        }

        if (request.DefaultNamespace != null)
        {
            cluster.DefaultNamespace = request.DefaultNamespace;
        }

        cluster.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(cluster);

        return MapToResponse(cluster);
    }

    public async Task DeleteAsync(Guid id)
    {
        var cluster = await _repository.GetByIdAsync(id);
        if (cluster == null)
        {
            throw new KeyNotFoundException($"Cluster with ID '{id}' not found.");
        }

        await _repository.DeleteAsync(cluster);
    }

    private static KubeClusterResponse MapToResponse(KubeCluster cluster)
    {
        return new KubeClusterResponse(
            cluster.Id,
            cluster.Name,
            cluster.Server,
            !string.IsNullOrEmpty(cluster.BearerTokenEnc), // HasBearerToken
            !string.IsNullOrEmpty(cluster.CertificateAuthorityPem), // HasCertificateAuthority
            cluster.InsecureSkipTlsVerify,
            cluster.DefaultNamespace,
            cluster.CreatedAt,
            cluster.UpdatedAt
        );
    }
}
