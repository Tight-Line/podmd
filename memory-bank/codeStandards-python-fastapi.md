# Backend Code Standards - Python FastAPI

## Purpose

These standards define coding, structure, and database conventions for **Python FastAPI applications**, focusing on asynchronous patterns, Pydantic models, SQLAlchemy ORM, and Swagger integration. They are optimized for scalable FastAPI applications that leverage asyncio, type hints, and modern Python patterns.

---

## 1. Style Guide Summary

### Python Conventions

- **Style:** Follow PEP 8 with black formatting and isort for imports.
  - Enable strict type hints with `mypy` for static type checking.
  - Prefer async/await patterns throughout the codebase.
  - Use dataclasses or Pydantic for data structures where appropriate.
  - Prefer pathlib over os.path for file operations.
- **Language version:** Python 3.9+ with modern features enabled.
- **Async patterns:** All database operations, external API calls, and I/O operations must be async.

### Linting & Formatting

- **Black:** Use black with 88-character line length for consistent formatting.
- **isort:** Organize imports with isort (imports grouped by standard/library, third-party, local).
- **flake8:** PEP 8 compliance with custom line-length (88 characters).
- **mypy:** Strict type checking enabled (`--strict` mode).
- **Optional:** `pylint` for additional code quality checks, document exceptions.
- **Pre-commit hooks:** Enable black, isort, flake8, and mypy hooks.

### Type Hints

- Use comprehensive type hints throughout the codebase.
- Use `typing` module for complex types (Union, Optional, List, Dict, etc.).
- Enable `from __future__ import annotations` for forward references if needed.
- Use generic types appropriately for better type safety.

---

## 2. File Structure Standards

Organize by app modules with clear separation between concerns: models, schemas, routers, services, and utilities.

### Top-Level Layout for Web APIs

- `app/` – Main application package
  - `main.py` – FastAPI application and startup configuration
  - `database.py` – SQLAlchemy async engine and session management
  - `auth.py` – Authentication utilities (JWT, password hashing)
  - `models/` – SQLAlchemy ORM models
  - `schemas/` – Pydantic request/response models
  - `routers/` – FastAPI route handlers organized by feature
  - `services/` – Business logic and external integrations
  - `utils/` – Shared utilities and helpers
- `alembic/` – Database migration scripts (if using SQLAlchemy)
- `tests/` – Unit and integration tests
- `docs/` – API documentation and guides

### File-to-Purpose Mapping

| File / Folder       | Purpose                                           |
| ------------------- | ------------------------------------------------- |
| `app/main.py`       | FastAPI app creation, middleware, routing         |
| `app/database.py`   | SQLAlchemy async engine, Base class, dependencies |
| `app/auth.py`       | JWT token handling, password hashing, auth utils  |
| `app/models/`       | SQLAlchemy declarative models                     |
| `app/schemas/`      | Pydantic models for requests/responses            |
| `app/routers/`      | FastAPI route handlers (auth, users, etc.)        |
| `app/services/`     | Business logic, API clients, integrations         |
| `alembic/versions/` | Database migration scripts                        |
| `tests/`            | Pytest test files and fixtures                    |

### Naming Conventions

- **Modules:** snake_case (e.g., `user_service.py`, `auth_router.py`)
- **Classes:** PascalCase (e.g., `UserService`, `AuthRouter`)
- **Functions/Methods:** snake_case (e.g., `get_user_by_id`, `create_access_token`)
- **Constants:** SCREAMING_SNAKE_CASE (e.g., `ACCESS_TOKEN_EXPIRE_MINUTES`)
- **Database Tables:** snake_case, plural (e.g., `users`, `posts`, `orders`)

### File Organization

- One public class per file in models, services, and routers.
- Group related schemas in single files (e.g., `user.py` for all user schemas).
- Use `__init__.py` files for package initialization and imports.
- Keep utility functions in dedicated modules within `utils/`.

---

## 3. FastAPI Application Standards

### App Configuration

- **main.py:** Keep minimal - import and configure FastAPI app, add middleware, include routers.
- **Middleware:** Add CORS, authentication, logging, and error handling middleware.
- **Startup/Shutdown Events:** Use lifespan events for database initialization and cleanup.
- **Root Route:** Provide basic API information endpoint (`/`).

### Routing

- **Routers:** Organize by domain/feature (auth, users, products, etc.).
- **Dependencies:** Use FastAPI dependency injection for database sessions and authentication.
- **Versioning:** Use URL prefixes for API versioning (`/v1/`).
- **Tags:** Add appropriate tags to routes for Swagger documentation.

### Error Handling

- Use FastAPI's `HTTPException` for API errors.
- Implement global exception handlers for common error types.
- Return consistent error response format with appropriate HTTP status codes.

---

## 4. Authentication Standards (JWT + Password Hashing)

### JWT Implementation

- Use `python-jose` (`jose` library) for JWT operations.
- Store secret key in environment variables (never in code).
- Set appropriate token expiration times (access: 30 mins, refresh: 7 days).
- Include essential claims (sub, exp, iat) in tokens.

### Password Security

- Use `passlib` with Argon2 for password hashing.
- Implement password policies (minimum length, complexity).
- Use cryptographic random salts provided by Argon2.

### Auth Patterns

- `get_current_user` dependency for protected routes.
- Optional authentication for routes that work with/without auth.
- Clear separation between authentication logic and authorization.

---

## 5. Database Standards (SQLAlchemy Async + Alembic)

### SQLAlchemy Configuration

- Use async SQLAlchemy with `AsyncSession` and async engines.
- Configure async PostgreSQL driver (`postgresql+asyncpg://`).
- Use `async_sessionmaker` for session management.
- Implement proper async context managers for session handling.

### Model Design

- Extend `Base` from `database.py` for all models.
- Use UUID primary keys for distributed systems.
- Include standard audit columns: `created_at`, `updated_at`.
- Use appropriate PostgreSQL-specific types (`UUID`, `TIMESTAMP WITH TIME ZONE`).

### Relationship Patterns

- Use async relationship loading (selectin, joined, etc.).
- Define foreign key constraints explicitly.
- Use string-based relationships for forward references.

### Schema Standards

- PascalCase for table and column names in models.
- Use clear, descriptive constraint names.
- Implement database-level validation where appropriate.

### Alembic Migrations

- Name migrations descriptively (e.g., `add_orders_table`).
- Generate migrations automatically with `alembic revision --autogenerate`.
- Test migrations in development before committing.
- Run migrations on startup in development environment only.

### Database Session Management

- Use FastAPI dependency injection (`get_db` dependency).
- Always use async context managers for session lifecycles.
- Implement proper transaction management with rollback on errors.

---

## 6. Pydantic Schema Standards

### Schema Design

- Use `BaseModel` for all schemas with comprehensive type hints.
- Leverage Pydantic validators and field configuration.
- Use `Config.from_attributes = True` for ORM model compatibility.
- Implement nested schemas for complex responses.

### Naming Patterns

- `Base`: Shared fields (e.g., `UserBase`)
- `Create`: Request schemas for creation operations
- `Update`: Request schemas for update operations
- `Response`: Response schemas with all fields
- `List`: Specialized schemas for list responses

### Validation

- Use Pydantic's built-in validators for common validations.
- Implement custom validators for business logic validation.
- Provide clear validation error messages.

---

## 7. Service Layer Standards

### Service Organization

- One service class per domain/feature.
- Clear separation between data access and business logic.
- Use dependency injection for database sessions and external clients.

### External Integrations

- **Kubernetes:** Use official `kubernetes` library with async operations.
- **OpenAI:** Use `openai` library with proper error handling and retries.
- **HTTP Clients:** Use `httpx` for async HTTP requests.
- Implement circuit breaker patterns for external API calls.

### Error Handling

- Raise appropriate exceptions for different error conditions.
- Implement retry logic for transient failures.
- Log errors with appropriate context information.

---

## 8. Dependency Management

### Requirements Organization

- Use `requirements.txt` for pinned production dependencies.
- Separate development dependencies (`requirements-dev.txt`).
- Pin all dependencies to specific versions in production.
- Regularly update and audit dependencies for security.

### Key Dependencies Standards

- **FastAPI:** Core web framework
- **Uvicorn:** ASGI server with standard options
- **Pydantic[email]:** Data validation with email support
- **SQLAlchemy[asyncio]:** Async ORM
- **AsyncPG:** PostgreSQL async driver
- **Alembic:** Database migrations
- **python-dotenv:** Environment variable management
- **python-jose[cryptography]:** JWT tokens
- **passlib[argon2]:** Password hashing
- **httpx:** Async HTTP client
- **kubernetes:** Kubernetes Python client
- **openai:** OpenAI API client

---

## 9. Testing Standards

### Test Framework

- Use `pytest` with `pytest-asyncio` for async tests.
- Use `pytest-cov` for code coverage reporting.
- Aim for 80%+ test coverage.

### Test Organization

- Mirror app structure in `tests/` directory.
- Use descriptive test names (e.g., `test_create_user_success`).
- Implement fixtures for common test setup (database session, client).

### Test Types

- **Unit Tests:** Test individual functions and services.
- **Integration Tests:** Test API endpoints and database interactions.
- **End-to-End Tests:** Test complete user workflows.

---

## 10. Environment & Configuration

### Environment Variables

- Store all configuration in environment variables.
- Use `python-dotenv` for local development (.env files).
- Never commit sensitive values to version control.

### Configuration Management

- Load environment variables in `main.py` or dedicated config module.
- Provide sensible defaults for non-sensitive configuration.
- Validate required environment variables on startup.

### Docker Standards

- Use multi-stage Docker builds for optimized images.
- Include only production dependencies in final image.
- Use non-root user for security.
- Implement proper health checks in docker-compose.yml.
