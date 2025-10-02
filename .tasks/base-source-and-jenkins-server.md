# **Task: Implement Base Source Class and Jenkins Server Build Source Type**

## **Objective Summary**

Add a new source type called "Jenkins Server builds" for CI/CD log analysis, with authentication requiring Jenkins server URL, username, and API token. Refactor existing KubeClusters to extract shared configuration data into a common base "Source" entity class.

## **Scope Summary**

- **Included**: JenkinsServers table, CRUD endpoints for Jenkins server configuration, database migration, changes to KubeClusters entity, new Sources table concept via base class
- **Excluded**: Log retrieval and analysis functionality for Jenkins server builds (configuration management only)

## **Entity Specifications**

### **Base Source Entity:**

- id (Guid, required) - Primary key
- name (string, required, unique) - Display name for the source
- type (string, required) - Source type: "Kubernetes" or "Jenkins"
- server (string, required, URL) - Base URL for the source
- keyVersion (int, required) - Encryption key version for sensitive data
- instructions (string, optional) - Special instructions for this source
- responseFormat (string, optional) - Response format for analysis
- createdAt (DateTime, required, auto) - Creation timestamp
- updatedAt (DateTime, required, auto) - Last update timestamp

### **KubeCluster Entity (inherits from Source):**

- Inherits all Source fields
- bearerTokenEnc (string, required, encrypted) - Encrypted Kubernetes bearer token
- certificateAuthorityPem (string, optional) - CA certificate for TLS verification
- insecureSkipTlsVerify (bool, required) - Skip TLS verification flag
- defaultNamespace (string, optional) - Default Kubernetes namespace

### **JenkinsServers Entity (inherits from Source):**

- Inherits all Source fields
- username (string, required) - Jenkins username for API authentication
- apiTokenEnc (string, required, encrypted) - Encrypted Jenkins API token

## **DTO Specifications**

### **Base SourceCreateDto:**

- name (string, required)
- type (string, required: "Kubernetes" or "Jenkins")
- server (string, required, URL)
- instructions (string, optional)
- responseFormat (string, optional)

### **KubeClustersCreateDto:**

- Inherits SourceCreateDto fields
- bearerToken (string, required) - Before encryption
- certificateAuthorityPem (string, optional)
- insecureSkipTlsVerify (bool, required)
- defaultNamespace (string, optional)

### **JenkinsServersCreateDto:**

- Inherits SourceCreateDto fields
- username (string, required)
- apiToken (string, required) - Before encryption

### **Update DTOs:** Similar to Create but all fields optional

### **Read DTOs (metadata-only):**

- SourceReadDto: id, name, type, server, instructions, responseFormat, createdAt, updatedAt
- KubeClustersReadDto: + certificateAuthorityPem, insecureSkipTlsVerify, defaultNamespace
- JenkinsServersReadDto: + username (excludes apiTokenEnc)

## **API Endpoint Specifications**

### **Kubernetes Clusters Endpoints:**

- GET /api/v1/clusters - List Kubernetes clusters (metadata only)
- GET /api/v1/clusters/{id} - Get cluster by ID
- POST /api/v1/clusters - Create new cluster
- PUT /api/v1/clusters/{id} - Update cluster
- DELETE /api/v1/clusters/{id} - Delete cluster

### **Jenkins Servers Endpoints (new):**

- GET /api/v1/jenkins-servers - List Jenkins servers
- GET /api/v1/jenkins-servers/{id} - Get Jenkins server by ID
- POST /api/v1/jenkins-servers - Create new Jenkins server
- PUT /api/v1/jenkins-servers/{id} - Update Jenkins server
- DELETE /api/v1/jenkins-servers/{id} - Delete Jenkins server

All endpoints require JWT authentication and follow metadata-only response pattern.

## **File Structure and Implementation Guidance**

### **New Files:**

- PodMD.Domain/Entities/Source.cs - Base entity class
- PodMD.Domain/Entities/JenkinsServers.cs - Jenkins entity inheriting from Source
- PodMD.Application/Dtos/SourceDto.cs - Base DTO classes
- PodMD.Application/Dtos/JenkinsServersDtos.cs - Jenkins DTOs
- PodMD.Application/Interfaces/IJenkinsServersRepository.cs - Repository interface
- PodMD.Application/Interfaces/IJenkinsServersService.cs - Service interface
- PodMD.Application/Services/JenkinsServersService.cs - Service with encryption
- PodMD.Infrastructure/Repositories/JenkinsServersRepository.cs - Repository implementation
- PodMD.Api/Controllers/V1/JenkinsServersController.cs - API controller
- PodMD.Api/Dtos/JenkinsServersDtos.cs - API DTOs
- PodMD.Infrastructure/Migrations/[Timestamp]\_AddJenkinsServers.cs - EF migration

### **Files to Modify:**

- PodMD.Domain/Entities/KubeCluster.cs - Make it inherit from Source
- PodMD.Application/Dtos/KubeClustersDtos.cs - Adjust to base DTO inheritance
- PodMD.Api/Program.cs - Add DI registrations for Jenkins services
- PodMD.Infrastructure/Persistence/ApplicationDbContext.cs - Add JenkinsServers DbSet

## **Implementation Steps**

1. Create base Source entity with shared properties
2. Modify KubeCluster entity to inherit from Source (remove duplicate properties)
3. Create JenkinsServers entity inheriting from Source
4. Update all related DTOs to follow inheritance pattern
5. Implement repository and service layer for Jenkins servers
6. Create JenkinsServersController with full CRUD endpoints
7. Generate database migration for schema changes
8. Update dependency injection registrations
9. Test all endpoints and ensure backward compatibility
10. Update memory bank documentation

## **Technical Requirements**

- Clean Architecture layering (Domain → Application → Infrastructure → API)
- EF Core code-first migrations for schema evolution
- AES-GCM encryption for sensitive credentials (Jenkins API tokens)
- Metadata-only API responses (no encrypted data returned)
- JWT Bearer authentication for all endpoints
- ProblemDetails responses for error handling
- Swagger/OpenAPI documentation
- Environment-based configuration

## **Assumptions**

- Jenkins servers are used for CI/CD build analysis (configuration only for this task)
- Existing KubeClusters data integrity will be maintained during refactoring
- Encryption service already handles Jenkins API tokens similar to Kubernetes tokens
- Database migration will handle inheritance properly (TPH or separate tables)
- No conflicts with existing unique name constraints

## **Validation and Testing**

- Entity validation mirrors existing KubeCluster patterns
- API request validation follows current standards
- Database constraints prevent duplicates and ensure referential integrity
- Backward compatibility with existing cluster endpoints
- New Jenkins endpoints follow same swagger documentation patterns
