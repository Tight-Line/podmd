# System Patterns for PodMD

## System Architecture

### Overall Architecture

- **Decision**: Clean Architecture with four-layer separation
- **Layers**: API → Application → Domain → Infrastructure
- **Purpose**: Maintain separation of concerns, testability, and framework independence

### Component Organization

- **API Layer**: ASP.NET Core controllers, middleware, and external interfaces
- **Application Layer**: Use cases, services, and business logic orchestration
- **Domain Layer**: Business entities, domain services, and core business rules
- **Infrastructure Layer**: External dependencies, data persistence, and integrations

## Key Technical Decisions (IMPLEMENTED ✅)

### Framework & Runtime

- **Decision**: .NET 9.0 with C# 13 for modern language features and performance
- **Status**: Successfully implemented with Clean Architecture
- **Purpose**: Long-term support, ecosystem maturity, and developer productivity

### Database & Persistence

- **Decision**: MySQL 8.0 with Entity Framework Core 9.0 and Pomelo provider
- **Status**: Fully operational with migrations applied and Identity tables created
- **Purpose**: Relational integrity, enterprise adoption, and LINQ query capabilities

### API Design & Documentation

- **Decision**: RESTful API with OpenAPI/Swagger and semantic versioning
- **Status**: Implemented with `/api/v1/*` endpoints and comprehensive Swagger docs
- **Purpose**: Standard interfaces, automatic documentation, and API evolution

### Error Handling & Responses

- **Decision**: ProblemDetails (RFC 7807) for standardized error responses
- **Status**: Implemented across all API endpoints with consistent formatting
- **Purpose**: Standardized error communication and API compliance

### Authentication & Security

- **Decision**: JWT Bearer tokens with ASP.NET Core Identity and RBAC
- **Status**: Production-ready with password hashing, claims, and role management
- **Purpose**: Stateless authentication, fine-grained authorization, and security best practices

### Data Encryption & Security

- **Decision**: AES-GCM encryption for sensitive data with Base64 encoding
- **Status**: Implemented for Kubernetes bearer tokens and Jenkins API tokens with key versioning
- **Purpose**: Secure storage of sensitive credentials and API keys
- **Key Management**: Environment-based configuration with 32-byte keys

### Multi-Source Architecture

- **Decision**: Extensible source type system with base Source entity and TPT inheritance
- **Status**: Successfully implemented for Kubernetes and Jenkins sources
- **Purpose**: Support multiple CI/CD platforms with consistent management
- **Pattern**: Table-Per-Type (TPT) inheritance providing separate tables for optimal queries
- **Benefits**: Zero-breaking expansion, type-safe discrimination, and query performance

### Inheritance Patterns

- **Decision**: Entity Framework Core Table-Per-Type (TPT) for multi-table inheritance
- **Status**: Configured with proper foreign key relationships and cascade deletion
- **Purpose**: Scalable entity hierarchies with separate physical tables
- **Benefits**: Superior query performance and cleaner separation of concerns

## Design Patterns in Use

### Architectural Patterns

- **Clean Architecture**: Dependency inversion and layer isolation
- **Domain-Driven Design**: Rich domain models with business logic in entities
- **CQRS**: Separate read/write models for complex operations
- **Repository Pattern**: Data access abstraction and testability

### Behavioral Patterns

- **Strategy Pattern**: Pluggable LLM providers and analysis algorithms
- **Factory Pattern**: Creation of complex objects (Kubernetes clients, encryption services)
- **Observer Pattern**: Event-driven architecture for async operations

### Structural Patterns

- **Adapter Pattern**: External service integrations (Kubernetes API, GitLab API)
- **Decorator Pattern**: Cross-cutting concerns (logging, caching, validation)
- **Composite Pattern**: Hierarchical organization of analysis results

## Component Relationships

### Layer Dependencies

- **API Layer** → Application Layer (controllers call application services)
- **Application Layer** → Domain Layer (services use domain entities and rules)
- **Infrastructure Layer** → Domain Layer (implements domain interfaces)
- **Domain Layer** → No dependencies (independent of other layers)

### Service Interactions

- **Controllers** inject and orchestrate **Application Services**
- **Application Services** coordinate **Domain Services** and **Infrastructure Services**
- **Infrastructure Services** implement interfaces defined in **Application** and **Domain** layers

### Data Flow

- **Input**: API Controllers → DTOs → Application Services
- **Processing**: Application Services → Domain Entities → Business Rules
- **Output**: Domain Entities → DTOs → API Responses
- **Persistence**: Infrastructure Repositories → EF Core → Database

## Critical Implementation Paths

### Log Analysis Flow with RAG

1. **API Request**: Client submits log analysis request with source details
2. **Authentication**: JWT validation and RBAC permission check
3. **RAG Context Retrieval**: Query connected knowledge bases for relevant context
4. **Knowledge Processing**: Extract, chunk, and process all knowledge base documents
5. **Context Integration**: Include ALL chunks within token limits in LLM prompt
6. **Source Integration**: Connect to Kubernetes/GitLab via encrypted credentials
7. **Log Retrieval**: Fetch logs using appropriate client (K8s API, GitLab API)
8. **Enhanced Analysis**: LLM processes logs enriched with domain-specific knowledge
9. **Response Generation**: Format structured analysis with context-aware recommendations
10. **Audit Logging**: Record analysis event with knowledge base usage metrics

### Authentication Flow

1. **Login Request**: User provides credentials via login endpoint
2. **Identity Validation**: ASP.NET Core Identity verifies credentials
3. **Token Generation**: Create JWT with user claims and roles
4. **Token Response**: Return JWT for subsequent API calls
5. **Token Validation**: All protected endpoints validate JWT and check permissions

### Data Persistence Flow

1. **Business Operation**: Application service performs business logic
2. **Repository Call**: Service calls repository interface method
3. **EF Core Execution**: Repository implementation uses EF Core for database operations
4. **Transaction Management**: EF Core handles transaction consistency
5. **Audit Trail**: Changes logged to audit tables for compliance

### Error Handling Flow

1. **Exception Occurrence**: Any layer throws exception during processing
2. **Global Handler**: ASP.NET Core middleware catches unhandled exceptions
3. **Error Logging**: Structured logging captures error details and context
4. **Response Formatting**: Consistent error response format returned to client
5. **Monitoring Alert**: Critical errors trigger monitoring alerts

## Frontend-Backend Integration

### API Communication

- **Decision**: RESTful API consumption with axios and centralized API client
- **Purpose**: Consistent API calls, error handling, and request/response interceptors

### State Management

- **Decision**: Pinia stores with composition API for reactive state
- **Purpose**: Predictable state updates and component reactivity

### Authentication Flow

- **Decision**: JWT token storage in localStorage with automatic refresh
- **Purpose**: Secure session management and seamless user experience

## Cross-Cutting Concerns

### Logging Strategy

- **Decision**: Structured logging with Serilog and contextual properties
- **Purpose**: Consistent log format, searchable logs, and debugging capabilities

### Configuration Management

- **Decision**: Environment-based configuration with validation
- **Purpose**: Secure credential management and environment-specific settings

### Health Monitoring

- **Decision**: ASP.NET Core Health Checks with database and external service monitoring
- **Purpose**: Proactive issue detection and system reliability assurance

### Performance Optimization

- **Decision**: Async/await patterns, connection pooling, and caching strategies
- **Purpose**: Scalable performance and responsive user experience
