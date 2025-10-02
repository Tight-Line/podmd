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
        // Validate HTTPS URL
        ValidationHelper.ValidateHttpsUrl(request.Server);

        // Encrypt the bearer token
        var encryptedToken = _encryptionService.Encrypt(request.BearerToken);

        var cluster = new KubeCluster
        {
            Type = "Kubernetes", // Explicit type for TPT inheritance
            Name = request.Name,
            Server = request.Server,
            BearerTokenEnc = encryptedToken,
            CertificateAuthorityPem = request.CertificateAuthorityPem,
            InsecureSkipTlsVerify = request.InsecureSkipTlsVerify,
            DefaultNamespace = request.DefaultNamespace,
            Instructions = request.Instructions,
            ResponseFormat = request.ResponseFormat,
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

        // Validate HTTPS URL if server is being changed
        if (!string.IsNullOrEmpty(request.Server))
        {
            ValidationHelper.ValidateHttpsUrl(request.Server);
            cluster.Server = request.Server;
        }

        // Encrypt new bearer token if provided
        if (!string.IsNullOrEmpty(request.BearerToken))
        {
            cluster.BearerTokenEnc = _encryptionService.Encrypt(request.BearerToken);
        }

        // Update other fields
        if (!string.IsNullOrEmpty(request.Name))
        {
            cluster.Name = request.Name;
        }

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

        if (request.Instructions != null)
        {
            cluster.Instructions = request.Instructions;
        }

        if (request.ResponseFormat != null)
        {
            cluster.ResponseFormat = request.ResponseFormat;
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
        var sourceDto = new SourceReadDto(
            cluster.Id,
            cluster.Name,
            cluster.Type,
            cluster.Server,
            cluster.Instructions,
            cluster.ResponseFormat,
            cluster.CreatedAt,
            cluster.UpdatedAt
        );

        return new KubeClusterResponse(
            sourceDto,
            !string.IsNullOrEmpty(cluster.CertificateAuthorityPem), // HasCertificateAuthority
            cluster.InsecureSkipTlsVerify,
            cluster.DefaultNamespace
        );
    }
}
