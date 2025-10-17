# Technical Context - Python FastAPI - PodMD

## Technologies Used

### Backend Framework

- **FastAPI 0.100+** - High-performance async web framework
- **Python 3.11** - Latest stable Python with performance improvements
- **Uvicorn 0.23+** - ASGI server with async worker support
- **Pydantic V2** - Data validation and parsing with JSON Schema generation
- **Purpose**: Modern Python web framework optimized for async operations and API documentation

### Database & Persistence

- **PostgreSQL 15+** - Advanced open-source relational database
- **SQLAlchemy 2.0+** - Python SQL toolkit with async support
- **AsyncPG 0.29+** - High-performance PostgreSQL async driver
- **Alembic 1.12+** - Database migration tool with async support
- **Purpose**: Robust relational data storage with async performance

### Authentication & Security

- **Python-JOSE 3.3+** - JWT token handling with cryptography support
- **Passlib 1.7+** - Password hashing with Argon2 algorithm
- **OAuth2 with JWT** - Token-based authentication system
- **Purpose**: Secure user authentication with modern cryptographic standards

### AI/LLM Integration

- **OpenAI Python Client** - Official OpenAI API client for chat completions
- **Purpose**: Integration with LLMs for log analysis

### External API Integrations

- **Kubernetes Python Client** - Official K8s API client library
- **JenkinsAPI** - REST client for Jenkins CI/CD server interaction
- **GitLab Python API** - REST client for GitLab CI/CD integration
- **httpx** - Unified async HTTP client for all external APIs
- **Purpose**: Multi-source log retrieval from Kubernetes and CI/CD platforms

### Development Tools

- **Poetry 1.7+** - Python dependency management and packaging
- **pytest 7.4+** - Comprehensive testing framework with async support
- **pytest-asyncio** - Async test fixtures and coroutines
- **Black 23+** - Python code formatter for consistent style
- **isort 5.12+** - Import statement organizer
- **mypy 1.6+** - Static type checker with strict mode
- **Purpose**: Modern Python development workflow with quality assurance

### Containerization & DevOps

- **Docker 24+** - Container platform for consistent deployment
- **Docker Compose 2.20+** - Multi-service development orchestration
- **Purpose**: Development and local testing environment

## Development Setup

### Environment

- **SDK**: Python 3.11+ with virtual environments
- **Database**: PostgreSQL 15+ running locally or in Docker
- **Containerization**: Docker Desktop with Compose V2
- **IDE**: VS Code with Python, Pylance, and testing extensions

### Local Development Workflow

- **Poetry** for dependency management and virtual environment
- **Docker Compose** for database and external services
- **Hot reload** with Uvicorn for API development
- **pytest** with coverage reporting for test execution
- **Alembic** for database schema evolution

### Environment Configuration

- **python-dotenv** for local .env file loading
- **pydantic-settings** for type-safe configuration validation
- **Environment-specific settings** (dev, staging, prod)
- **Secret management** with file-based keys for development

## Technical Constraints

### Database

- **Connection pooling** with asyncpg for concurrent connections
- **Transaction management** with SQLAlchemy async sessions
- **Migration safety** with pre/post deployment validations
- **Backup automation** before schema changes

### Performance

- **Async/await everywhere** for I/O bound operations
- **Connection pooling** for external API calls
- **Circuit breakers** for resilient external service dependencies
- **Rate limiting** using token bucket algorithms
- **Memory management** with streaming for large log files

### Security

- **Input validation** with Pydantic models preventing injection
- **Password policies** with complexity requirements
- **JWT token expiration** with refresh token rotation
- **HTTPS everywhere** in production environments
- **CORS configuration** for cross-origin in development

### Scalability

- **Horizontal scaling** with stateless API design
- **Database connection pooling** for multiple worker processes

## Dependencies

### Core Framework

- **fastapi[all]==0.100.0**: Web framework with docs dependencies
- **uvicorn[standard]==0.23.0**: ASGI server with hot reload
- **pydantic[email]==2.4.0**: Data validation with email support
- **pydantic-settings==2.1.0**: Configuration management

### Database & ORM

- **sqlalchemy[asyncio]==2.0.0**: Async SQL toolkit
- **asyncpg==0.29.0**: PostgreSQL async driver
- **alembic==1.12.0**: Database migration tool
- **psycopg2-binary==2.9.7**: Sync driver for Alembic

### Authentication & Security

- **python-jose[cryptography]==3.3.0**: JWT token handling
- **passlib[argon2]==1.7.4**: Password hashing
- **bcrypt==4.0.1**: Alternative password hashing
- **cryptography==41.0.0**: General cryptographic operations

### External Integrations

- **kubernetes==28.1.0**: Kubernetes API client
- **jenkinsapi==0.3.13**: Jenkins REST client
- **python-gitlab==3.15.0**: GitLab API client
- **httpx[http2]==0.25.0**: Async HTTP client
- **openai==1.3.0**: OpenAI API client

### Development & Testing

- **pytest==7.4.0**: Testing framework
- **pytest-asyncio==0.21.0**: Async test support
- **pytest-cov==4.1.0**: Coverage reporting
- **httpx==0.25.0**: Test client for FastAPI
- **faker==20.0.0**: Test data generation

### Quality Assurance

- **black==23.9.0**: Code formatting
- **isort==5.12.0**: Import sorting
- **mypy==1.6.0**: Type checking
- **flake8==6.1.0**: Linting
- **pre-commit==3.5.0**: Pre-commit hooks

### Containerization

- **uv==0.1.0**: Fast Python package installer for containers
- **docker-compose**: Multi-service orchestration

## Tool Usage Patterns

### FastAPI Application

- **Dependency injection** via FastAPI's DI system
- **Request/response lifecycle** with middleware integration
- **Route organization** by feature with APIRouter
- **Exception handling** with custom HTTPException classes
- **Background tasks** for async processing separation

### Database Operations

- **Async context managers** for session lifecycle management
- **Repository pattern** for data access abstraction
- **Unit of work** for transaction management
- **Connection pooling** for performance optimization
- **Migration scripts** for schema evolution

### Authentication Flow

- **JWT creation** with configurable expiration
- **Password verification** with Argon2 async hashing
- **Token validation** middleware for protected routes
- **Refresh token rotation** for security maintenance
- **Role-based access** with permission checking

### Testing Strategy

- **pytest fixtures** for setup/teardown automation
- **Async test functions** for concurrent operations
- **Mocked dependencies** for isolated unit testing
- **Integration tests** with TestClient and live database
- **Coverage reporting** with minimum thresholds

### External API Integration

- **HTTP clients** with timeout and retry configuration
- **Error normalization** across different API providers
- **Rate limiting** to respect third-party API limits
- **Connection pooling** for performance optimization
- **Circuit breaker patterns** for fault tolerance

### Development Workflow

- **Poetry environments** for reproducible dependencies
- **Hot reload development** with file watching
- **Pre-commit hooks** for code quality enforcement
- **Docker Compose** for integrated development setup
- **IDE integration** with debugging and testing support

This technical context defines the complete technology stack and development practices for building PodMD as a Python FastAPI application from scratch, ensuring consistency across the development lifecycle.
