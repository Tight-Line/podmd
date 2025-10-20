# Active Context - PodMD

## Current Work Focus

**Phase 4: Kubernetes Clusters CRUD - COMPLETE** ✅

Full Kubernetes cluster management API implemented with JWT authentication, encrypted token storage, Pydantic validation, and comprehensive error handling. All 5 CRUD endpoints tested and working. Ready to advance to actual external integrations and analysis engine.

## Recent Changes

- **Phase 3 JWT Authentication Implementation Complete**: Secure JWT auth endpoints with user registration/login, Argon2 password hashing, token generation, and comprehensive error handling
- **PR Updated**: Draft PR #6 updated for feature/jwt-auth-endpoints-5 implementing Issue #5 with final commits
- **Endpoints Validated**: Docker-tested registration (201), login (200 with JWT token), error handling (409/401)
- **Security Implementation**: JWT HS256 tokens, Argon2 hashing (m=65536,t=3,p=4), email validation, unique constraints
- **Database Integration**: PostgreSQL with async SQLAlchemy, proper session management, health endpoint DB connectivity
- **Code Standards Validated**: All implementation follows established async patterns and conventions

- **Phase 2 Models Implementation Complete**: User, Source, KubeCluster SQLAlchemy models with async Alembic migrations implemented
- **PR Created**: Draft PR #4 opened for feature/add-user-source-kubecluster-models-3 implementing Issue #3
- **Infrastructure Established**: Alembic async migration setup, auto-updating timestamps, cascade relationships
- **Code Standards Validated**: All implementation follows established patterns and conventions
- **GitHub Workflow**: Issue → Branch → Implementation → PR process fully demonstrated (2nd time)

## Next Steps

### Immediate Next Steps

- **Transition to Phase 3**: Begin Authentication & Security implementation
- **Authentication Endpoints**: Implement JWT token generation/validation endpoints
- **User Registration**: Password hashing with Argon2 and secure user creation
- **Security Middleware**: JWT validation and CORS configuration
- **Role-Based Access**: Basic permission system foundation

### Development Phase Planning

2. **Phase 2: Core Models & Authentication** - **START HERE**

   - SQLAlchemy user, role, and permission models
   - JWT authentication endpoints (login, register, refresh)
   - Basic security middleware and CORS

3. **Phase 3: Source Integration Foundation**

   - Source provider models (K8s, Jenkins, GitLab configurations)
   - Credential storage and encryption
   - Health check endpoints for source validation

### Development Order Considerations

1. **Authentication & Security** (foundation layer - required for all other features)
2. **Source Management** (credential storage and validation - prerequisite for analysis)
3. **Analysis Engine** (core business logic - depends on auth + sources)
4. **Web Frontend** (user interface - depends on complete API)
5. **Integration Testing** (end-to-end workflows)

## Active Decisions and Considerations

### Technology Stack Finalized

- **Framework**: FastAPI with async patterns (confirmed optimal for concurrent log fetching/analysis)
- **Database**: PostgreSQL with asyncpg (selected for reliability and async performance)
- **Auth**: JWT with Argon2 password hashing (balanced security and performance)
- **Integrations**: httpx for async HTTP, kubernetes/python-gitlab/jenkinsapi clients

### Architecture Patterns Established

- **Layered Architecture**: API → Service → Repository → Persistence (maintains clean separation)
- **Async-First Design**: All I/O operations async (critical for concurrent external API calls)
- **Repository Pattern**: Data access abstraction for testability and flexibility
- **Service Layer**: Business logic coordination and external integrations

### Implementation Trade-offs Evalued

- **Authentication**: Chose stateless JWT over sessions for scalability (no sticky sessions required)
- **Database Relations**: Using eager loading strategies for analysis endpoints (balance performance vs. complexity)
- **API Versioning**: URL-based versioning (/v1/) for clarity and evolution
- **Error Handling**: FastAPI HTTPException with structured Problem Details for API consistency

## Important Patterns and Preferences

### Code Organization

- **Module Structure**: app/ with clear separation (models/, schemas/, routers/, services/, utils/)
- **Naming Conventions**: snake_case files/functions, PascalCase classes, SCREAMING_SNAKE constants
- **Import Organization**: Standard library → Third-party → Local, sorted alphabetically

### Async Patterns

- **Database Operations**: Always use AsyncSession with async context managers
- **External Calls**: httpx async client with timeout/retry configuration
- **Concurrent Operations**: asyncio.gather for parallel log fetching from multiple sources

### Security Priorities

- **Credential Storage**: AES-GCM encryption with key rotation for stored API tokens
- **Input Validation**: Comprehensive Pydantic schemas for all API inputs
- **Logging Standards**: Structured logging with sensitive data redaction (no tokens/keys logged)

### Development Workflow

- **Testing Strategy**: pytest-asyncio for async tests, aim for 80%+ coverage
- **Code Quality**: Black formatting, isort imports, mypy strict typing
- **Documentation**: Auto-generated OpenAPI docs, comprehensive docstrings

## Important Patterns and Preferences

### Code Organization

- **Module Structure**: app/ with clear separation (models/, schemas/, routers/, services/, utils/)
- **Naming Conventions**: snake_case files/functions, PascalCase classes, SCREAMING_SNAKE constants
- **Import Organization**: Standard library → Third-party → Local, sorted alphabetically

### Async Patterns

- **Database Operations**: Always use AsyncSession with async context managers
- **External Calls**: httpx async client with timeout/retry configuration
- **Concurrent Operations**: asyncio.gather for parallel log fetching from multiple sources
- **Auto-Updating Timestamps**: SQLAlchemy event listeners for created_at/updated_at with UTC datetimes

### Security Priorities

- **Credential Storage**: AES-GCM encryption with key rotation for stored API tokens
- **Input Validation**: Comprehensive Pydantic schemas for all API inputs
- **Logging Standards**: Structured logging with sensitive data redaction (no tokens/keys logged)

### Development Workflow

- **Testing Strategy**: pytest-asyncio for async tests, aim for 80%+ coverage
- **Code Quality**: Black formatting, isort imports, mypy strict typing
- **Documentation**: Auto-generated OpenAPI docs, comprehensive docstrings

## Learnings and Project Insights

### Documentation Approach

- **Memory Bank Essential**: Session-reset dependency makes comprehensive documentation critical for continuity
- **Hierarchical Structure**: PB→PC→SP→TC→AC→P creates logical knowledge flow
- **Detail-Level Balance**: Technical docs (SP, TC) enable immediate implementation without rediscovery

### Architecture Decisions

- **Async Importance**: Multiple concurrent integrations (K8s, Jenkins, GitLab, LLM) demand async design
- **Security First**: SRE/DevOps users handle sensitive credentials - robust security mandatory
- **Scalability Considerations**: Multi-tenant design with horizontal scaling from day one
- **Async Alembic Patterns**: Established async env.py configuration with proper connection handling

### Implementation Challenges

- **Asyncpg Compatibility**: Python 3.13 compatibility resolved with pyenv and Poetry environment management
- **Alembic Async Setup**: Required lambda function wrapper for SQLAlchemy async context manager in migration scripts
- **Model Relationships**: Cascade deletes and foreign key constraints properly configured for data integrity

### Development Readiness

- **Complete Foundation**: All architectural decisions documented before coding begins
- **Standards Established**: Code standards defined prevent immediate style debates
- **Clear Starting Point**: Memory Bank completeness enables focused implementation without context gaps

## Open Questions and Dependencies

- **LLM Integration Choice**: Which LLM providers beyond OpenAI (Anthropic, local models)? Architecture prepared for provider abstraction
- **Frontend Framework**: React vs Vue mentioned in scope - Vue.js aligned with CodePenTool tips, but validate user preference
- **Deployment Strategy**: Docker compose confirmed, but target platform (K8s, cloud service) not specified
- **Scalability Targets**: Expected concurrent users/analysis jobs not quantified - may influence threading/model choices

## Risk Considerations

- **External API Reliability**: K8s/Jenkins/GitLab APIs may have rate limits or downtime - circuit breaker patterns prepared
- **AI Response Consistency**: LLM responses vary by input context - confidence scoring and evidence citation may need refinement
- **Knowledge Base Scale**: RAG integration assumes substantial documentation available - may need initial content strategy
- **Credential Security**: Multi-tenant credential storage carries responsibility - encryption and audit logging critical
