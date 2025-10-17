# System Patterns for Python FastAPI - PodMD

## System Architecture

### Overall Architecture

- **Decision**: Layered Architecture (API → Service → Repository → Persistence)
- **Framework**: FastAPI with async patterns throughout
- **Database**: PostgreSQL with SQLAlchemy async
- **Purpose**: High-concurrency API for real-time log analysis

### Component Organization

- **API Layer**: FastAPI routers with dependency injection
- **Service Layer**: Business logic and external API integrations
- **Repository Layer**: Data access abstraction
- **Domain Layer**: Core business models and validation

## Key Technical Decisions

### Framework & Runtime

- **Decision**: Python 3.9+ with FastAPI 0.100+ and Uvicorn ASGI server
- **Database**: PostgreSQL with asyncpg driver for high performance
- **ORM**: SQLAlchemy 2.0+ with async support
- **Purpose**: Async-first design for concurrent log fetching and analysis

### Authentication & Security

- **Decision**: JWT Bearer tokens with refresh token rotation
- **Password Hashing**: Argon2 via passlib
- **Purpose**: Secure access to sensitive infrastructure credentials

### API Design Patterns

- **Decision**: RESTful API with OpenAPI 3.0 documentation
- **Versioning**: URL-based versioning (e.g., `/v1/`)
- **Serialization**: Pydantic V2 for automatic validation and docs
- **Purpose**: Developer-friendly API with client SDK generation

### Data Persistence Strategy

- **Decision**: Repository pattern with Unit of Work for transactions
- **Migrations**: Alembic for schema evolution
- **Connection**: Async connection pooling for concurrent requests
- **Purpose**: Reliable data operations under load

### External Integration Patterns

- **Decision**: Async HTTP clients (httpx) for K8s/Jenkins APIs
- **Authentication**: Encrypted JWT tokens for service credentials
- **Circuit Breaking**: Tenacity for resilient external calls
- **Purpose**: Robust integration with external systems

## Design Patterns in Use

### Architectural Patterns

- **Repository Pattern**: Abstract data access interfaces
- **Dependency Injection**: FastAPI's DI for service composition
- **CQRS**: Separate command/query models for complex operations

### Behavioral Patterns

- **Strategy Pattern**: Pluggable CI providers (Jenkins, GitLab)
- **Factory Pattern**: LLM provider factories (OpenAI, Anthropic)
- **Observer Pattern**: Event-driven analysis pipeline

### Structural Patterns

- **Adapter Pattern**: Unified interface for different log sources
- **Façade Pattern**: Simplified API for complex log analysis
- **Composite Pattern**: Hierarchical error analysis structure

## Component Relationships

### Layer Dependencies

- **Routers** depend on **Services** via dependency injection
- **Services** depend on **Repositories** for data access
- **Repositories** depend on **SQLAlchemy Models**
- **Services** coordinate **External Clients** for integrations
- **No circular dependencies** - unidirectional flow

### Request Flow Lifecycle

1. **API Gateway**: FastAPI router validates input with Pydantic
2. **Security**: JWT validation middleware checks permissions
3. **Service Coordination**: Business logic orchestrates operations
4. **Data Access**: Repository layer manages persistence
5. **External Integration**: Concurrent API calls to k8s/jenkins
6. **LLM Processing**: Async analysis with context enrichment
7. **Response Assembly**: Structured results via validated schemas

## Critical Implementation Paths

### Core Log Analysis Flow

1. **Request Validation**: Pydantic validates analysis request
2. **Authentication**: Verify user access to specified cluster/project
3. **Knowledge Retrieval**: Async query vectorized knowledge bases
4. **Source Authentication**: Decrypt and validate service credentials
5. **Concurrent Log Fetching**: Parallel requests to pod logs and CI outputs
6. **RAG Context Building**: Chunk and rank relevant knowledge
7. **LLM Synthesis**: Structured analysis with step-by-step fixes
8. **Audit Logging**: Record analysis for compliance and billing

### Authentication Sequence

1. **Login Endpoint**: Accept username/password via secure schema
2. **Credential Verification**: Async password hashing verification
3. **Token Minting**: Generate JWT with claims and refresh token
4. **Secure Storage**: HTTPOnly cookies for token persistence
5. **Permission Loading**: Database query for user role assignments

### Multi-Source Abstract Flow

1. **Provider Registration**: Store encrypted connection details
2. **Health Validation**: Test connectivity on registration
3. **Client Factory**: Instantiate appropriate API client by type
4. **Log Retrieval**: Transform PodMD requests to provider-native APIs
5. **Error Normalization**: Consistent error handling across providers
6. **Connection Pooling**: Reuse connections for performance

## Cross-Cutting Concerns

### Error Handling Strategy

- **Decision**: Global exception handlers with Problem Details (RFC 7807)
- **Classification**: HTTP status codes map to error types
- **Purpose**: Consistent API error responses across all endpoints

### Configuration Management

- **Decision**: Environment-based configuration with pydantic-settings
- **Validation**: Runtime config validation on startup
- **Purpose**: Type-safe environment variable handling

### Monitoring & Observability

- **Decision**: FastAPI middleware for request tracing
- **Metrics**: Response times, error rates, integration health
- **Purpose**: Operational visibility for production reliability

### Testing Foundations

- **Decision**: pytest with async fixtures for unit/integration testing
- **Mocking**: HTTP external service mocking for reliable tests
- **Purpose**: CI/CD pipeline confidence with comprehensive test coverage

## Infrastructure & DevOps Patterns

### Containerization Strategy

- **Decision**: Multi-stage Docker builds with uv for fast Python packaging
- **Base Images**: python:3.11-alpine for minimal production images
- **Purpose**: Optimized CI/CD pipeline and deployment efficiency

### Environment Management

- **Decision**: Dev/Stage/Prod environment configurations
- **Secrets**: External vault integration for production credentials
- **Purpose**: Consistent environment handling across deployment stages

### Database Migration Strategy

- **Decision**: Alembic with pre/post deployment hooks
- **Backup**: Automatic schema backup before migrations
- **Purpose**: Safe, reversible database schema evolution

### Scalability Patterns

- **Decision**: Horizontal pod scaling with Redis session storage
- **Rate Limiting**: Token bucket algorithm for API protection
- **Purpose**: Auto-scaling during traffic spikes and DoS protection

## Security Patterns

### Data Encryption

- **Decision**: AES-GCM for credential encryption with key rotation
- **Storage**: Separate keystore for encryption keys
- **Purpose**: Secure credential management and compliance readiness

### API Security

- **Decision**: Rate limiting, CORS configuration, and input sanitization
- **Headers**: Security headers (HSTS, CSP, X-Frame-Options)
- **Purpose**: Protection against common web vulnerabilities

### Access Control

- **Decision**: Role-based access control (RBAC) with resource-level permissions
- **Policies**: Centralized policy engine for authorization decisions
- **Purpose**: Fine-grained access control for multi-tenant architecture

This initial system patterns document provides the architectural foundation for building PodMD as a scalable log analysis platform, emphasizing async patterns, clean architecture, and prepared adaptability for the planned RAG integration and CI/CD extensions.
