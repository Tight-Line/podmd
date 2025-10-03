# KnowledgeBase Entity Feature Specification & Implementation Task

## 🚀 Task Overview

**Objective:** Introduce a `KnowledgeBase` entity with full CRUD REST endpoints backed by MySQL persistence, establishing a many-to-many relationship between KnowledgeBase and Source entities.

**Key Requirements:**

- Full CRUD operations (Create, Read, Update, Delete)
- Many-to-many relationship with existing Source entity
- MySQL persistence via Entity Framework Core
- RESTful API following existing patterns
- Clean Architecture compliance

---

## 📋 Scope & Specifications

### ✅ In Scope

- KnowledgeBase entity definition and EF Core configuration
- DTOs for Create, Update, and Read operations
- RESTful CRUD endpoints in KnowledgeBasesController
- Service layer with validation, uniqueness checks, and mapping
- Swagger documentation with example schemas
- Error handling via ProblemDetails (400 for validation, 409 for conflicts)
- Database migration for schema and many-to-many junction table

### ❌ Out of Scope

- File uploads and embedding/vector store functionality
- Frontend components (Vue.js interface)
- Authentication scopes or roles beyond existing JWT Bearer
- Advanced RAG capabilities
- Content processing or parsing

---

## 🔧 Technical Specifications

### Entity Design (PodMD.Domain/Entities/KnowledgeBase.cs)

| Field Name  | Data Type           | Required | Constraints         | Description               |
| ----------- | ------------------- | -------- | ------------------- | ------------------------- |
| Id          | Guid                | Required | Primary Key         | Unique identifier         |
| Name        | string              | Required | Unique, Max 100     | Knowledge base name       |
| Description | string              | Optional | Max 1000            | Optional description      |
| CreatedAt   | DateTime            | Required | Auto-generated      | Creation timestamp        |
| UpdatedAt   | DateTime            | Required | Auto-generated      | Last update timestamp     |
| Sources     | ICollection<Source> | -        | Navigation Property | Many-to-many relationship |

### DTO Specifications

#### CreateKnowledgeBaseDto (PodMD.Application/Dtos/KnowledgeBasesDtos.cs)

- `Name` (string, required) - Name of the knowledge base
- `Description` (string, optional) - Optional description

#### UpdateKnowledgeBaseDto

- `Id` (Guid, required) - Knowledge base identifier
- `Name` (string, required) - Updated name
- `Description` (string, optional) - Updated description

#### KnowledgeBaseDto (Read Response)

- `Id` (Guid) - Primary key
- `Name` (string) - Knowledge base name
- `Description` (string) - Description if available
- `CreatedAt` (DateTime) - Creation timestamp
- `UpdatedAt` (DateTime) - Last update timestamp

### API Endpoints (PodMD.Api/Controllers/V1/KnowledgeBasesController.cs)

| Method | Path                           | Request Body           | Response           | Status Codes       | Authentication |
| ------ | ------------------------------ | ---------------------- | ------------------ | ------------------ | -------------- |
| GET    | `/api/v1/knowledge-bases`      | -                      | KnowledgeBaseDto[] | 200                | JWT Bearer     |
| GET    | `/api/v1/knowledge-bases/{id}` | -                      | KnowledgeBaseDto   | 200, 404           | JWT Bearer     |
| POST   | `/api/v1/knowledge-bases`      | CreateKnowledgeBaseDto | KnowledgeBaseDto   | 201, 400, 409      | JWT Bearer     |
| PUT    | `/api/v1/knowledge-bases/{id}` | UpdateKnowledgeBaseDto | KnowledgeBaseDto   | 200, 400, 404, 409 | JWT Bearer     |
| DELETE | `/api/v1/knowledge-bases/{id}` | -                      | -                  | 204, 404           | JWT Bearer     |

---

## 📁 Files to Create/Modify

### New Files

- `PodMD.Domain/Entities/KnowledgeBase.cs` - Entity definition with navigation property
- `PodMD.Application/Dtos/KnowledgeBasesDtos.cs` - CRUD DTOs
- `PodMD.Api/Controllers/V1/KnowledgeBasesController.cs` - RESTful API controller
- `PodMD.Application/Services/KnowledgeBasesService.cs` - Business logic layer
- `PodMD.Application/Interfaces/IKnowledgeBasesService.cs` - Service interface
- `PodMD.Application/Interfaces/IKnowledgeBasesRepository.cs` - Repository interface
- `PodMD.Infrastructure/Repositories/KnowledgeBasesRepository.cs` - EF Core implementation
- `PodMD.Infrastructure/Migrations/2025XXXXXXXX_AddKnowledgeBase.cs` - Migration file

### Modified Files

- `PodMD.Infrastructure/Persistence/ApplicationDbContext.cs` - Add DbSet and relationship config
- `PodMD.Api/Program.cs` - Add DI registrations for service and repository

---

## 🏗️ Implementation Steps

### 1. Domain Layer

1. Create `KnowledgeBase.cs` entity in Domain layer
   - Include navigation property for Sources
   - Add standard audit fields
   - Include uniqueness constraint on Name

### 2. Application Layer

1. Define DTOs in Application layer
   - CreateKnowledgeBaseDto
   - UpdateKnowledgeBaseDto
   - KnowledgeBaseDto
2. Create service interface and implementation
3. Create repository interface

### 3. Infrastructure Layer

1. Create repository implementation
2. Update ApplicationDbContext
   - Add KnowledgeBase DbSet
   - Configure many-to-many relationship with Source
3. Create EF Core migration

### 4. API Layer

1. Create KnowledgeBasesController
   - Implement CRUD endpoints
   - Add routing, authorization, validation
2. Update Program.cs for DI
   - Register service and repository
   - Ensure Singleton for repository patterns per existing code

### 5. Testing & Validation

1. Build application
2. Verify migration applies
3. Test endpoints with Swagger UI
4. Verify relationships work properly

---

## ⚡ Assumptions & Technical Decisions

### Database Schema Decisions

- **Many-to-many implementation**: EF Core automatic junction table (KnowledgeBaseSource)
- **Table naming**: KnowledgeBases (pluralized by convention)
- **Constraints**: Unique constraint on Name field
- **Audit fields**: Standard CreatedAt/UpdatedAt pattern matching Source entity

### API Design Decisions

- **Version namespace**: `/api/v1/` following existing pattern
- **Controller location**: Controllers/V1/ directory
- **Response format**: ProblemDetails for errors, DTOs for success
- **Authentication**: Consistent JWT Bearer across all endpoints

### Architecture Compliance

- **Clean Architecture**: Respects Domain→Application→Infrastructure layers
- **Repository Pattern**: Separates business logic from data access
- **Dependency Injection**: Constructor injection with proper scoping
- **Error Handling**: Application layer validation with API layer translation

---

## 🧪 Testing Strategy

### End-to-End Validation

1. **Database**: Verify migration creates tables correctly
2. **Relationships**: Test SQL foreign keys and constraints
3. **Endpoints**: Swagger UI availability and response schemas
4. **Authentication**: JWT Bearer token validation on all endpoints

### Integration Points

1. **Many-to-many**: CRUD operations preserve Source relationships
2. **DI Container**: Service and repository resolve properly
3. **EF Core**: Queries include relationship data where needed

---

## 📝 Success Criteria

- [ ] KnowledgeBase entity successfully persisted in MySQL
- [ ] CRUD endpoints return correct HTTP status codes
- [ ] Swagger documentation generated automatically
- [ ] Many-to-many relationship functions with Source entities
- [ ] Unique constraint on Name field enforced
- [ ] All endpoints require JWT Bearer authentication
- [ ] Error responses use ProblemDetails format
- [ ] Code compiles without warnings
- [ ] Daily Docker development environment runs successfully

---

## 🔌 Dependencies & Prerequisites

- Existing Source entity and infrastructure
- EF Core 9.0 migration tools
- MySQL 8.0 database running
- JWT Bearer authentication configured
- Clean Architecture layers in place

---

## ⚠️ Risk Mitigation

### Migration Rollback

- Test migration in development before production
- Keep schema scripts for emergency rollback

### Relationship Constraints

- Verify Source entities exist before KnowledgeBase operations
- Handle cascade deletions appropriately

### Performance Considerations

- Monitor query performance with relationship includes
- Consider indexing strategy if needed

---

## 📚 References

- **Project Patterns**: Review Source entity implementation for consistency
- **Clean Architecture**: Domain→Application→Infrastructure layers
- **EF Core**: Many-to-many relationship configuration
- **API Conventions**: Existing cluster/jobs endpoints for reference

---

## ✅ Implementation Checklist

### Domain Layer

- [ ] Create KnowledgeBase entity
- [ ] Add navigation property for Sources
- [ ] Include data annotations and constraints

### Application Layer

- [ ] Create DTOs (Create, Update, Read)
- [ ] Implement service with validation logic
- [ ] Create repository interface

### Infrastructure Layer

- [ ] Implement repository with EF operations
- [ ] Update ApplicationDbContext
- [ ] Create and apply migration

### API Layer

- [ ] Create RESTful controller
- [ ] Implement CRUD endpoints with proper routing
- [ ] Register dependencies in DI container

### Validation

- [ ] Build application successfully
- [ ] Run migration to create database schema
- [ ] Test endpoints via Swagger UI
- [ ] Verify authentication and authorization

Document any deviations from this specification in comments.
