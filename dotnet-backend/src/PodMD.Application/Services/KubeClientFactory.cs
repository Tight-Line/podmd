using System.Security.Cryptography.X509Certificates;
using k8s;
using PodMD.Application.Interfaces;
using PodMD.Domain.Entities;

namespace PodMD.Application.Services;

public class KubeClientFactory : IKubeClientFactory
{
    private readonly IKubeClusterRepository _clusterRepository;
    private readonly EncryptionService _encryptionService;

    public KubeClientFactory(
        IKubeClusterRepository clusterRepository,
        EncryptionService encryptionService)
    {
        _clusterRepository = clusterRepository;
        _encryptionService = encryptionService;
    }

    public async Task<IKubernetes> CreateClientAsync(Guid clusterId)
    {
        var cluster = await _clusterRepository.GetByIdAsync(clusterId);
        if (cluster == null)
        {
            throw new KeyNotFoundException($"Cluster with ID '{clusterId}' not found.");
        }

        // Decrypt the bearer token
        var bearerToken = _encryptionService.Decrypt(cluster.BearerTokenEnc);

        // Configure Kubernetes client
        var config = new KubernetesClientConfiguration
        {
            Host = cluster.Server,
            AccessToken = bearerToken,
            SkipTlsVerify = cluster.InsecureSkipTlsVerify
        };

        // Handle CA certificate if provided
        if (!string.IsNullOrEmpty(cluster.CertificateAuthorityPem))
        {
            // For PEM format, we need to handle it appropriately
            // KubernetesClient expects the CA data in a specific format
            config.SslCaCerts = new X509Certificate2Collection
            {
                X509CertificateLoader.LoadCertificate(System.Text.Encoding.UTF8.GetBytes(cluster.CertificateAuthorityPem))
            };
        }

        return new Kubernetes(config);
    }
}
