# Kubernetes Log Retrieval Implementation Task

## Overview

Implement services and API endpoints to retrieve logs from configured Kubernetes clusters, supporting:

- Fetching pod logs by namespace and pod name
- Fetching logs for deployments by identifying failed pods and returning the failing container logs
  Use KubernetesClient SDK.

## Objective

Enable log retrieval from Kubernetes clusters via API endpoints using the KubernetesClient SDK for programmatic access to cluster resources and logs.

## Scope

**Must Include:**

- DTOs for log requests and responses
- API endpoints accepting namespace, pod/deployment names in the request body
- Validation and error handling for invalid inputs, failed pod selection, and container ambiguity

**Excluded:**

- Log streaming, events/watches, advanced analysis, multi-tenancy features

## Entity Specifications

### KubeCluster Entity (Existing)

| Field                   | Data Type | Required | Constraints    | Description                               |
| ----------------------- | --------- | -------- | -------------- | ----------------------------------------- |
| Id                      | Guid      | Yes      | Primary Key    | Unique identifier for the cluster         |
| Name                    | string    | Yes      | MaxLength(100) | Cluster name                              |
| Server                  | string    | Yes      | URL format     | Kubernetes API server URL                 |
| BearerTokenEnc          | string    | Yes      | -              | Encrypted bearer token for authentication |
| CertificateAuthorityPem | string?   | No       | -              | Optional CA certificate in PEM format     |
| InsecureSkipTlsVerify   | bool      | Yes      | -              | Skip TLS certificate verification         |
| DefaultNamespace        | string?   | No       | -              | Default namespace for operations          |
| KeyVersion              | int       | Yes      | -              | Encryption key version                    |
| CreatedAt               | DateTime  | Yes      | -              | Record creation timestamp                 |
| UpdatedAt               | DateTime  | Yes      | -              | Record update timestamp                   |

**Notes:**

- Used for retrieving cluster credentials for Kubernetes client creation
- TLS configuration supports system trust store (default), optional CA PEM, or skip TLS verification

## DTO Specifications

Standard CRUD DTOs skipped - log-specific DTOs to be defined during implementation including:

- PodLogRequest: clusterId, namespace, podName, containerName?, tailLines?, sinceSeconds?, previous?, limitBytes?, includeDescription?
- DeploymentLogRequest: clusterId, namespace, deploymentName, fallback?, includeDescription?
- LogResponse: logs (string), description? (string), metadata (pod/container info)

## API Endpoint Specifications

| Method | Path                                       | Description                                                                                                                                  | Request Body                                                                                                                 | Response                                         | Status Codes                                                                                   | Authentication   |
| ------ | ------------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------ | ---------------------------------------------------------------------------------------------- | ---------------- |
| POST   | /api/clusters/{clusterId}/pods/logs        | Return logs for a specific pod with optional parameters (tailLines, sinceSeconds, previous, limitBytes). Optionally include pod description. | Pod log request (namespace, podName, containerName?, tailLines?, sinceSeconds?, previous?, limitBytes?, includeDescription?) | Log content with optional pod description        | 200 (success), 400 (validation error), 404 (pod not found), 502 (upstream error)               | JWT Bearer Token |
| POST   | /api/clusters/{clusterId}/deployments/logs | Return logs for a deployment's failed pod, selecting failing container with fallback support. Optionally include deployment description.     | Deployment log request (namespace, deploymentName, fallback?, includeDescription?)                                           | Log content with optional deployment description | 200 (success), 400 (ambiguous container/validation), 404 (no failed pod), 502 (upstream error) | JWT Bearer Token |

**Constraints:**

- Validation: required fields, tailLines > 0, deployment must have failed pod
- Returns 400 for ambiguous container, 404 if no failed pod (fallback=false), 502 for upstream errors
- Environment variables: cluster credentials pulled from database, decrypted for client
- Secrets not redacted for this task

## Implementation Guidance

### Architecture Alignment

- Follow Clean Architecture: API → Application → Domain → Infrastructure
- Use KubernetesClient SDK for all Kubernetes operations
- Implement factory pattern for Kubernetes client creation
- Decrypt stored credentials before client creation
- Handle TLS configuration (CA PEM, skip verify, system trust)

### Key Implementation Points

1. **Client Factory:**

   - Retrieve KubeCluster from database by clusterId
   - Decrypt bearer token using existing encryption service
   - Configure KubernetesClient with server URL, token, and TLS settings
   - Handle CA certificate parsing and trust store configuration

2. **Log Service:**

   - Implement pod log retrieval with optional parameters
   - Implement deployment log retrieval with failed pod detection
   - Handle container selection for multi-container pods
   - Support fallback options for deployment logs
   - Optionally include resource descriptions (pod/deployment specs)

3. **API Controller:**
   - Validate cluster access permissions
   - Parse request parameters with defaults from appsettings
   - Handle various error scenarios with appropriate HTTP status codes
   - Return structured responses with logs and optional metadata

### Error Handling

- 400 Bad Request: Invalid parameters, ambiguous containers
- 404 Not Found: Pod/deployment not found, no failed pods
- 502 Bad Gateway: Kubernetes API errors, connection failures
- Include detailed error messages for debugging

### Security Considerations

- JWT authentication required for all endpoints
- Validate user has access to specified cluster
- Decrypt credentials securely in memory only
- No sensitive data in logs (as per requirements)

### Dependencies

- Add KubernetesClient package to PodMD.Api.csproj
- Ensure encryption service is available for token decryption
- Use existing KubeCluster repository for cluster data access

## Testing Requirements

1. **Unit Tests:**

   - Client factory credential decryption
   - Log service parameter validation
   - Error handling scenarios

2. **Integration Tests:**

   - Full log retrieval flow with mock Kubernetes API
   - Authentication and authorization
   - Error response formatting

3. **Manual Testing:**
   - Test with real Kubernetes cluster (if available)
   - Verify parameter handling and defaults
   - Check Swagger documentation

## Assumptions and Requirements

- KubernetesClient SDK is compatible with .NET 9.0
- Existing encryption service handles token decryption
- KubeCluster repository provides cluster data access
- JWT authentication is properly configured
- Appsettings can be modified for default values

## Success Criteria

1. Pod logs can be retrieved by namespace/pod name
2. Deployment logs return failing container logs
3. Proper error handling for all edge cases
4. JWT authentication works on endpoints
5. Application builds and runs without errors
6. Logs are returned in expected format

## Next Steps

1. Add KubernetesClient NuGet package
2. Implement IKubeClientFactory and KubeClientFactory
3. Implement IKubeLogService and KubeLogService
4. Create LogsController with endpoints
5. Add default settings to appsettings.json
6. Test the implementation thoroughly
7. Update API documentation
