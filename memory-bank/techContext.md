# Technical Context - PodMD

## Technologies Used

### Backend Framework (IMPLEMENTED ✅)

- **.NET 9.0** with **C# 13** - Modern language features, performance, ecosystem
- **ASP.NET Core Web API** - RESTful API framework with controllers and routing
- **Entity Framework Core 9.0** - ORM with LINQ queries, migrations, and relationships
- **MySQL 8.0** - Relational database with transactions and indexing

### Authentication & Security (IMPLEMENTED ✅)

- **JWT Bearer Authentication** - Stateless token-based security with Swagger UI support
- **ASP.NET Core Identity** - User management with password hashing and claims
- **Serilog** - Structured logging with JSON output

### Containerization & DevOps (IMPLEMENTED ✅)

- **Docker** - Containerization for consistent environments
- **Docker Compose** - Multi-service orchestration (API + MySQL)
- **Health Checks** - ASP.NET Core health monitoring (Database + LLM service)
- **Environment Configuration** - Options pattern with .env file support and validation

### AI/LLM Integration (IMPLEMENTED ✅)

- **OpenAI API** - Compatible chat completions with exponential backoff
- **HttpClient** - Configurable HTTP client with timeout and retry logic
- **Configuration** - Environment-based LLM settings with validation (.env-dev)
- **Error Handling** - Comprehensive LLM exceptions and response parsing

### Development Tools (IMPLEMENTED ✅)

- **Swashbuckle/OpenAPI** - Automatic API documentation
- **Git** - Version control with conventional commits
- **Visual Studio Code** - Primary IDE with C# extensions

### Frontend Framework (IMPLEMENTED ✅)

- **Vue.js 3** with **Composition API** - Reactive UI framework with modern script setup syntax
- **PrimeVue 4** - Professional component library with Aura theme (@primeuix/themes)
- **TypeScript 5.9** - Type-safe JavaScript with strict type checking
- **Vite 7.1** - Fast build tool with Hot Module Replacement (HMR)
- **Pinia** - Intuitive state management for Vue with authentication store
- **Tailwind CSS 4** - Configuration-free framework with @tailwindcss/vite plugin
- **Vue Router** - Official routing library with navigation guards
- **Axios** - HTTP client with global authentication interceptors

### Testing & Quality (PLANNED - Not Started)

- **xUnit** - Unit testing framework with async test support
- **ASP.NET Core TestServer** - In-memory API testing

## Development Setup

### Environment

- **SDK**: .NET 9.0 SDK with C# 13 support
- **Database**: MySQL 8.0+ running locally or in Docker
- **Containerization**: Docker for consistent development environments

## Technical Constraints

### Database

- **UTF8MB4 encoding** for international character support
- **Foreign key constraints** for referential integrity
- **Encrypted fields** for sensitive data (passwords, API keys)
- **Connection pooling** for performance

### Performance

- **Async/await** for all I/O operations
- **EF Core query optimization** with Include/ThenInclude for relationships
- **Transaction management** for data consistency
- **Caching strategies** for frequently accessed data

### Security

- **Input validation** with meaningful error messages
- **SQL injection prevention** through parameterized queries
- **XSS protection** in any future web interfaces
- **Audit logging** for compliance requirements

## Dependencies

### Core Framework

- **Microsoft.AspNetCore** - Web API, routing, middleware
- **Microsoft.EntityFrameworkCore** - ORM functionality
- **Pomelo.EntityFrameworkCore.MySql** - MySQL database provider

### Authentication

- **Microsoft.AspNetCore.Identity** - User management
- **Microsoft.AspNetCore.Authentication.JwtBearer** - JWT token handling

### Development Tools

- **Swashbuckle.AspNetCore** - OpenAPI/Swagger documentation
- **Microsoft.Extensions.Diagnostics.HealthChecks** - Health monitoring
- **xunit** - Testing framework
- **Microsoft.AspNetCore.TestHost** - Integration testing

## Tool Usage Patterns

### Entity Framework Core

- **Code-first migrations** for schema evolution
- **Repository pattern** for data access abstraction
- **Async methods** for all database operations
- **Navigation properties** with lazy/eager loading

### ASP.NET Core

- **Dependency injection** with constructor injection
- **Middleware pipeline** for cross-cutting concerns
- **Configuration binding** to strongly-typed options
- **Global exception handling** with custom error responses

### Testing

- **Arrange-Act-Assert** pattern in all tests
- **In-memory database** for unit tests
- **TestServer** for API integration tests
- **Async test methods** for async operations

### Development Workflow

- **Git flow** branching strategy
- **Continuous integration** with automated testing
- **Environment-specific configuration** management
