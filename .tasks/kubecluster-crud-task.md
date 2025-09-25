# Kubernetes Cluster CRUD Feature Implementation Task

## Overview

### Objective

Persist and manage Kubernetes clusters with secure Bearer tokens and provide RESTful CRUD operations. All sensitive data is encrypted, and metadata-only responses are returned.

### Scope

**Include:**

- `KubeCluster` entity definition with AES-GCM encrypted Bearer token
- DTOs for Create, Update, and Read operations
- REST endpoints under `/api/clusters`
- Validation rules (HTTPS URLs, required fields, uniqueness)
- Metadata-only responses (flags for token/CA presence)

**Exclude:**

- Namespace listing, log retrieval, test-connection, other auth types, RBAC/multi-tenancy

### Success Criteria

- RESTful CRUD API endpoints for Kubernetes cluster management
- Secure encryption of sensitive data (Bearer tokens)
- Metadata-only responses for security
- JWT authentication required for all operations
- Clean Architecture compliance
- EF Core integration with existing MySQL database

## Entity Design

### KubeCluster Entity

| Field                   | Type     | Required | Constraints         | Description                         |
| ----------------------- | -------- | -------- | ------------------- | ----------------------------------- |
| Id                      | Guid     | Yes      | Primary Key         | Unique identifier                   |
| Name                    | string   | Yes      | Unique              | Human-friendly name                 |
| Server                  | string   | Yes      | HTTPS URL           | Cluster API endpoint                |
| BearerTokenEnc          | string   | Yes      | Encrypted (AES-GCM) | Encrypted bearer token              |
| CertificateAuthorityPem | string   | No       | -                   | Certificate authority in PEM format |
| InsecureSkipTlsVerify   | bool     | Yes      | -                   | Skip TLS verification flag          |
| DefaultNamespace        | string   | No       | -                   | Default Kubernetes namespace        |
| KeyVersion              | int      | Yes      | -                   | Encryption key version              |
| CreatedAt               | DateTime | Yes      | Auto                | Creation timestamp                  |
| UpdatedAt               | DateTime | Yes      | Auto                | Update timestamp                    |

## DTO Specifications

### CreateKubeClusterRequest

```csharp
public record CreateKubeClusterRequest(
    string Name,
    string Server,
    string BearerToken,
    string? CertificateAuthorityPem,
    bool InsecureSkipTlsVerify,
    string? DefaultNamespace
);
```

### UpdateKubeClusterRequest

```csharp
public record UpdateKubeClusterRequest(
    string? Name,
    string? Server,
    string? BearerToken,
    string? CertificateAuthorityPem,
    bool? InsecureSkipTlsVerify,
    string? DefaultNamespace
);
```

### KubeClusterResponse

```csharp
public record KubeClusterResponse(
    Guid Id,
    string Name,
    string Server,
    bool HasBearerToken,
    bool HasCertificateAuthority,
    bool InsecureSkipTlsVerify,
    string? DefaultNamespace,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
```

## API Endpoints

| Method | Endpoint           | Description                                 | Constraints                                   |
| ------ | ------------------ | ------------------------------------------- | --------------------------------------------- |
| POST   | /api/clusters      | Create a new Kubernetes cluster             | JWT authentication required, input validation |
| GET    | /api/clusters      | List all clusters (metadata-only responses) | JWT authentication required                   |
| GET    | /api/clusters/{id} | Get cluster by ID (metadata-only response)  | JWT authentication required                   |
| PUT    | /api/clusters/{id} | Update cluster information                  | JWT authentication required, input validation |
| DELETE | /api/clusters/{id} | Delete cluster                              | JWT authentication required                   |

## File Structure

### Files to Create

- `PodMD.Domain/Entities/KubeCluster.cs` → Entity definition with encrypted fields
- `PodMD.Application/Dtos/KubeClusterDtos.cs` → Create/Update/Read DTOs
- `PodMD.Application/Interfaces/IKubeClusterRepository.cs` → Repository interface
- `PodMD.Application/Interfaces/IKubeClusterService.cs` → Service interface
- `PodMD.Application/Services/KubeClusterService.cs` → Business logic and encryption
- `PodMD.Application/Services/EncryptionService.cs` → AES-GCM encryption/decryption
- `PodMD.Application/Configuration/EncryptionSettings.cs` → Encryption configuration
- `PodMD.Infrastructure/Repositories/KubeClusterRepository.cs` → EF Core repository implementation
- `PodMD.Api/Controllers/V1/ClustersController.cs` → REST API endpoints
- `PodMD.Infrastructure/Migrations/xxx_AddKubeCluster.cs` → Database migration

### Files to Modify

- `PodMD.Infrastructure/Persistence/ApplicationDbContext.cs` → Add DbSet and configurations
- `PodMD.Api/Program.cs` → Dependency injection setup
- `PodMD.Api/appsettings.json` → Encryption settings

## Implementation Steps

### 1. Domain Layer

1. Create `KubeCluster.cs` entity with all fields
2. Add data annotations for validation

### 2. Application Layer

1. Create DTOs in `KubeClusterDtos.cs`
2. Create `IKubeClusterRepository.cs` interface
3. Create `IKubeClusterService.cs` interface
4. Create `EncryptionSettings.cs` configuration class
5. Implement `EncryptionService.cs` with AES-GCM encryption
6. Implement `KubeClusterService.cs` with business logic

### 3. Infrastructure Layer

1. Create `KubeClusterRepository.cs` implementing EF Core patterns
2. Update `ApplicationDbContext.cs` to include KubeCluster DbSet
3. Create EF Core migration for KubeCluster table

### 4. API Layer

1. Create `ClustersController.cs` with CRUD endpoints
2. Update `Program.cs` for DI registration
3. Update `appsettings.json` with encryption configuration

### 5. Testing & Validation

1. Test encryption/decryption functionality
2. Verify API endpoints with Postman/Swagger
3. Test database operations
4. Validate security (no token exposure in responses)

## Assumptions and Requirements

### Assumptions (Step 7 Skipped - Standard Assumptions Applied)

- AES-GCM encryption library available in .NET (System.Security.Cryptography)
- Encryption keys stored securely in configuration (appsettings)
- No existing KubeCluster table conflicts in database
- JWT authentication already working for all endpoints
- HTTPS URL validation implemented with Uri validation
- Name uniqueness enforced at database level
- Clean Architecture layering maintained
- EF Core migrations handle schema changes safely

### Technical Requirements

- .NET 9.0 with C# 13
- MySQL 8.0 with EF Core 9.0
- JWT Bearer authentication
- AES-GCM encryption for sensitive data
- RESTful API design with OpenAPI documentation
- Clean Architecture (4-layer separation)
- Async/await patterns for all I/O operations

### Security Requirements

- Bearer tokens encrypted at rest
- Metadata-only responses (no sensitive data exposure)
- JWT authentication required for all operations
- Input validation and sanitization
- HTTPS URL validation for server endpoints
- Unique name constraints to prevent conflicts

## Project Context

### PodMD Overview

PodMD is a Web API service that automates troubleshooting for failed Kubernetes pods/deployments and CI builds. It fetches logs, enriches them with knowledge via Retrieval-Augmented Generation (RAG), and uses Large Language Models (LLMs) to produce structured, actionable "how to fix" recommendations.

### Current Status

- Backend API implementation: COMPLETE (85% overall progress)
- Clean Architecture: 5-project structure implemented
- Authentication: JWT + ASP.NET Core Identity working
- Database: MySQL 8.0 with EF Core 9.0, migrations applied
- Infrastructure: Docker containerization ready

### Architecture Patterns

- Clean Architecture with 4-layer separation (API → Application → Domain → Infrastructure)
- Repository pattern for data access
- Dependency injection throughout
- Async patterns for scalability
- Structured logging with Serilog

### Technology Stack

- Backend: .NET 9.0, ASP.NET Core Web API, EF Core 9.0, MySQL 8.0
- Authentication: JWT Bearer tokens with ASP.NET Core Identity
- Containerization: Docker with docker-compose
- Documentation: OpenAPI/Swagger

## Next Steps

1. Start implementation following the file structure above
2. Begin with Domain layer (KubeCluster entity)
3. Implement encryption service
4. Create repository and service layers
5. Build API controller with endpoints
6. Test thoroughly before integration
7. Update memory-bank with implementation details

## Risk Mitigation

- Implement encryption service first to validate AES-GCM approach
- Test API endpoints incrementally
- Ensure no sensitive data leaks in responses
- Validate database constraints and migrations
- Follow Clean Architecture principles strictly
- Document any deviations from standards

---

**Generated by Feature Specification Workflow**  
**Date:** 2025-09-24  
**Status:** Ready for Implementation
