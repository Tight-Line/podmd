# Knowledge Bases File Upload Foundation - MinIO File Storage

## Objective

Lay the foundation for uploading files to Knowledge Bases by creating entities, migrations, service layer, API endpoints, and Swagger docs. Add upload file logic to MinIO, but make it customizable so it can be switched to AWS or different storage in prod, include MinIO deployment in docker compose.

## Key Requirements

- **Foundation Phase:** Establish core infrastructure for file management
- **Entity & Database:** KnowledgeFile entity with EF Core config and migration
- **File Storage Abstraction:** IFileStorage interface with MinIO implementation (configurable for AWS/prod storage)
- **Metadata Focus:** Service validates and persists file metadata only
- **API CRUD Operations:** Upload, list, replace, delete endpoints
- **Documentation:** Swagger integration for all endpoints and DTOs
- **Deployment:** MinIO service in docker compose

## Scope

### Must Include

- KnowledgeFile entity with EF Core configuration and migration
- IFileStorage interface and implementation
- KnowledgeFileService for validating and persisting file metadata only
- KnowledgeFilesController with endpoints for upload, list, delete, and replace
- Swagger documentation for endpoints and DTOs

### Assumptions

- Entity conflicts checked via existing schema
- Default allowed file types: PDF, DOCX, TXT, etc.
- Max file size: 10MB per file
- Soft delete via IsDeleted flag
- JWT Bearer authentication (existing in project)

## Entity Specifications - KnowledgeFile

| Field           | Data Type    | Required | Constraints                          | Description                                 |
| --------------- | ------------ | -------- | ------------------------------------ | ------------------------------------------- |
| Id              | Guid         | Yes      | Primary Key (PK)                     | Unique identifier for the file              |
| KnowledgeBaseId | Guid         | Yes      | Foreign Key (FK to KnowledgeBase.Id) | Links file to its Knowledge Base            |
| FileName        | String       | Yes      | Max 255 chars                        | Original file name with extension           |
| StorageKey      | String       | Yes      | Unique                               | Storage path/key in MinIO or other provider |
| ContentType     | String       | Yes      | Max 100 chars                        | MIME type (e.g. "application/pdf")          |
| FileSize        | Int64 (long) | Yes      | > 0                                  | File size in bytes                          |
| CreatedAt       | DateTime     | Yes      | Auto-set                             | When the record was created                 |
| UpdatedAt       | DateTime     | Yes      | Auto-set                             | Last modification time                      |
| IsDeleted       | Bool         | No       | Default: false                       | Soft delete flag                            |

### Navigation Properties

- KnowledgeBase: Navigation to parent KnowledgeBase (from KnowledgeBase entity)

## DTO Specifications

### CreateKnowledgeFileDto

- KnowledgeBaseId (Guid, required) - ID of the Knowledge Base to associate with
- File (IFormFile, required) - The file to upload (binary data)

### UpdateKnowledgeFileDto (for replace/rename)

- FileName (string, optional) - New file name if renaming (max 255 chars)

### KnowledgeFileDto (Read/List Response)

- Id (Guid) - Primary key
- KnowledgeBaseId (Guid) - Associated Knowledge Base ID
- FileName (string) - File name (display name)
- ContentType (string) - MIME type
- FileSize (long) - Size in bytes
- CreatedAt (DateTime) - Creation timestamp
- UpdatedAt (DateTime) - Last update timestamp

### Special Handling

- Create/Update: Handles binary file data via IFormFile
- Read: Excludes internal StorageKey for security
- File validation: Size limits, type restrictions

## API Endpoints

### RESTful API v1 (/api/v1)

| Method | Path                                           | Action       | Description                                                | Request Body                                 | Response               |
| ------ | ---------------------------------------------- | ------------ | ---------------------------------------------------------- | -------------------------------------------- | ---------------------- |
| POST   | /api/v1/knowledgebases/{knowledgeBaseId}/files | Upload Files | Upload one or multiple files to a Knowledge Base           | CreateKnowledgeFileDto (multipart/form-data) | List<KnowledgeFileDto> |
| GET    | /api/v1/knowledgebases/{knowledgeBaseId}/files | List Files   | Get all files for a Knowledge Base, excluding soft deleted | None                                         | List<KnowledgeFileDto> |
| PUT    | /api/v1/files/{fileId}                         | Replace File | Replace file content with new upload                       | CreateKnowledgeFileDto                       | KnowledgeFileDto       |
| DELETE | /api/v1/files/{fileId}                         | Delete File  | Soft delete file (set IsDeleted=true)                      | None                                         | 204 No Content         |

### Constraints & Validation

- **Authentication:** All endpoints require JWT Bearer token
- **File Validation:**
  - Max size: 10MB per file
  - Allowed types: PDF, DOCX, TXT, CSV, JSON, MD, etc. (configurable)
  - Unique filenames per KnowledgeBase (optional)
- **Path Parameters:**
  - knowledgeBaseId: Valid Guid, must exist
  - fileId: Valid Guid, must exist and not deleted
- **Response Codes:**
  - 200: Success with response body
  - 201: Created (uploads)
  - 204: Deleted successfully
  - 400: Validation error
  - 401: Unauthorized
  - 404: Not found
  - 409: Conflict (duplicate filename)
  - 413: File too large

### Swagger Documentation Requirements

- OpenAPI 3.0 specification
- Include detailed descriptions, examples, and schemas
- Authentication: Bearer token in header
- Examples: Sample requests/responses with realistic data
- Validation: Show schema constraints and error responses

## File Structure & Implementation

### Domain Layer

- `PodMD.Domain/Entities/KnowledgeFile.cs` - Entity definition with attributes
- `PodMD.Domain/Interfaces/IFileStorage.cs` - Abstraction interface

### Application Layer

- `PodMD.Application/Dtos/KnowledgeFileDtos.cs` - All DTOs
- `PodMD.Application/Interfaces/IKnowledgeFileService.cs` - Service contract
- `PodMD.Application/Interfaces/IKnowledgeFileRepository.cs` - Repository contract
- `PodMD.Application/Services/KnowledgeFileService.cs` - Validation and metadata logic
- `PodMD.Application/Services/MinioFileStorage.cs` - IFileStorage implementation

### Infrastructure Layer

- `PodMD.Infrastructure/Migrations/2025XXXXXXXX_AddKnowledgeFile.cs` - EF migration
- `PodMD.Infrastructure/Persistence/ApplicationDbContext.cs` - Add KnowledgeFile DbSet
- `PodMD.Infrastructure/Repositories/KnowledgeFileRepository.cs` - EF implementation

### API Layer

- `PodMD.Api/Controllers/V1/KnowledgeFilesController.cs` - REST endpoints
- `PodMD.Api/Program.cs` - Register services, enable Swagger

### Infrastructure Setup

- `docker-compose.yml` - Add MinIO service, volumes, environment

## Implementation Steps

1. Create KnowledgeFile entity with EF attributes
2. Generate and run migration (check for conflicts)
3. Implement IFileStorage abstraction and MinIO implementation
4. Create DTOs for API operations
5. Implement service layer for metadata validation
6. Implement repository for database operations
7. Create controller with CRUD endpoints
8. Add MinIO to docker compose
9. Register dependencies in Program.cs
10. Test application builds and runs
11. Verify Swagger documentation generates correctly
12. Test endpoints with sample files

## Technical Requirements

- **Database:** PostgreSQL via EF Core, respecting existing migrations
- **File Storage:** MinIO for local dev, configurable for AWS S3 in prod
- **Logging:** Use existing logging infrastructure
- **Error Handling:** Consistent with project's error patterns
- **Validation:** FluentValidation if existing
- **Health Checks:** Include MinIO service health check
- **Environment Variables:** MinIO credentials via env vars
- **Security:** No hard-coded secrets, sensitive data encryption if needed

## Success Criteria

- [ ] Application builds without errors
- [ ] Database migration runs successfully
- [ ] MinIO starts in docker compose
- [ ] All endpoints respond correctly with proper auth
- [ ] File upload creates database record and stores in MinIO
- [ ] List returns file metadata (excluding storage paths if sensitive)
- [ ] Replace updates file content while preserving metadata
- [ ] Delete soft-deletes properly
- [ ] Swagger docs show complete API specification
- [ ] Service layer validates file constraints

## Cleanup & Maintenance

- Remove Color.Docx hard-coded values
- Ensure EF Core configurations are complete
- Verify relationships cascade correctly
- Add unit tests for service methods
- Update API documentation in README if needed

## Risks & Assumptions

- KnowledgeBase entity exists and has Id field
- EF migration naming follows pattern 2025XXXXXXXX\_
- MinIO service added doesn't conflict with existing services
- File types and sizes are reasonable defaults
- Authentication middleware already set up
- No existing file-conflicting entity names

## Documentation Update Requirements

After implementation, update memory-bank files:

- activeContext.md: Add file upload feature status
- progress.md: Document completion and progress
- systemPatterns.md: Note IFileStorage abstraction pattern
- Add new diagrams if needed
