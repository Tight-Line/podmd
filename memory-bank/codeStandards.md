# Code Standards - C#/.NET Web API

## Purpose

These standards define coding, structure, and database conventions for **C#/.NET Web API projects**, focusing on predictable folder layout, EF Core configuration, DTOs, controllers, and Swagger integration. They are optimized for automated generation tools (like Cline) while enforcing clean, maintainable code.

---

## 1. Style Guide Summary

### .NET / C# Conventions

- **Style:** Follow Microsoft .NET and C# coding conventions for .NET 9.
  - Enable nullable reference types (`Nullable = enable`).
  - Prefer file-scoped namespaces.
  - Prefer async/await, task-based async patterns.
  - Use pattern matching and expression-bodied members where clear.
  - Avoid preview features unless team-approved.
- **Language version:** Latest stable C#; opt-in to preview only by team decision.

### Linting

- **Analyzers:** Enable `Microsoft.CodeAnalysis.NetAnalyzers` with `AnalysisLevel = latest`.
- **Severity:** Warnings treated as errors in CI; local relaxations allowed.
- **Optional:** `StyleCop.Analyzers` for additional rules, document exceptions in `.editorconfig`.
- **Baselines:** Use scoped suppressions or SARIF baseline for legacy code.

### Formatting

- Indentation: 4 spaces.
- Braces: Allman style (opening brace on a new line).
- Line length: 120–140 columns.
- Line endings: LF enforced via `.gitattributes`.
- Encoding: UTF-8 without BOM.
- Use `.editorconfig` and `dotnet format`; do not commit unformatted code.

### Naming

- **PascalCase:** Types, interfaces (prefix `I`), methods, properties, events, constants.
- **camelCase:** Local variables, parameters.
- **\_camelCase:** Private/protected fields (including static).
- **Enums:** Singular type names; PascalCase members.
- **Async methods:** Suffix `Async`.
- **Acronyms:** PascalCase (HttpClient, XmlReader); SCREAMING_SNAKE_CASE only for env vars.

---

## 2. File Structure Standards

Organize by solution, projects, and feature/layer folders. Clear boundaries between API, Domain, Application, Infrastructure, and Shared.

### Top-Level Layout

- `ProjectName.sln`
- `src/`
  - `ProjectName.Api/` – Web API or Minimal APIs
  - `ProjectName.Domain/` – Domain entities, value objects, services
  - `ProjectName.Application/` – Use cases, commands/queries, interfaces/ports
  - `ProjectName.Infrastructure/` – EF Core, integrations, repositories
  - `ProjectName.Shared/` – Shared utilities
- `tests/`
  - `ProjectName.Api.Tests/`
  - `ProjectName.Domain.Tests/`
  - `ProjectName.Application.Tests/`
  - `ProjectName.Infrastructure.Tests/`
  - `ProjectName.Shared.Tests/`

### Controllers / Endpoints

- **MVC:** `src/ProjectName.Api/Controllers/V1/`
- **Minimal APIs:** `src/ProjectName.Api/Endpoints/V1/`

### Folder & Namespace Conventions

- **Folders:** PascalCase, match project/feature boundaries.
- **Files:** One public type per file; file name matches type.
- **Namespaces:** Mirror folder structure.
  - Example: `namespace ProjectName.Api.Controllers.V1.Feature;`

### File-to-Purpose Mapping

| File / Folder                                  | Purpose                                         |
| ---------------------------------------------- | ----------------------------------------------- |
| `src/ProjectName.Api/Program.cs`               | Entry point, minimal configuration              |
| `src/ProjectName.Api/Configuration/*.cs`       | DI, logging, Swagger, health checks             |
| `src/ProjectName.Api/Controllers/`             | MVC controllers, versioned folders (V1, V2)     |
| `src/ProjectName.Api/Endpoints/`               | Minimal API endpoints grouped by version        |
| `src/ProjectName.Domain/Entities/`             | Domain entities                                 |
| `src/ProjectName.Domain/ValueObjects/`         | Domain value objects                            |
| `src/ProjectName.Domain/Services/`             | Domain services and business logic              |
| `src/ProjectName.Application/Services/`        | Application services, use case logic            |
| `src/ProjectName.Application/Dtos/`            | DTOs for requests/responses                     |
| `src/ProjectName.Application/Interfaces/`      | Ports, repository interfaces, service contracts |
| `src/ProjectName.Infrastructure/Persistence/`  | DbContext, EF Core configurations, migrations   |
| `src/ProjectName.Infrastructure/Integrations/` | External clients / SDKs                         |
| `src/ProjectName.Shared/`                      | Shared utilities, helpers, constants            |
| `tests/`                                       | Unit and integration tests                      |

### Build Configuration (Directory.Build.props)

- Use `Directory.Build.props` in the solution root for common build settings.
- Define shared properties like `TargetFramework`, `LangVersion`, and `Nullable`.
- Configure common compiler warnings and code analysis rules.
- Include SonarAnalyzer for enhanced code quality analysis.
- Set assembly information (company, copyright, version).
- Define conditional compilation symbols if needed.
- Include common `Using` directives to reduce imports.
- Enable Container Orchestrator Support for Docker Compose project setup (Visual Studio).
- For VS Code: Manually create docker-compose.yml following the same service orchestration patterns.

### Package Version Management (Directory.Packages.props)

- Use `Directory.Packages.props` in the solution root for centralized package version management.
- Define all NuGet package versions in `<PackageVersion>` elements.
- Reference packages without version numbers in individual `.csproj` files.
- Update versions in one place for the entire solution.
- Include version ranges for patch-level updates (e.g., `[9.0.0, 10.0.0)`).
- Document version constraints and update policies.

### .gitignore Standards

- Include standard .NET ignores: `bin/`, `obj/`, `.vs/`, `packages/`
- Ignore `appsettings.Development.json` and other environment-specific secret files
- Ignore User Secrets: `.microsoft/usersecrets/`
- Ignore IDE files: `.vscode/`, `*.user`, `*.suo`
- Ignore OS files: `.DS_Store`, `Thumbs.db`
- Ignore build artifacts and temporary files
- Document any custom ignore rules in comments

### Configuration File Standards

- **Commit `appsettings.json`** with default values or placeholders for all configuration keys.
- **Ignore `appsettings.Development.json`** in `.gitignore` to prevent committing local secrets.
- **Use User Secrets** or environment variables for local development secrets and sensitive data.
- **Use `appsettings.{Environment}.json`** for environment-specific overrides.
- **Document all configuration keys** in comments or separate documentation.

### Configuration Standards (Options Pattern)

- Use `IOptions<T>`, `IOptionsSnapshot<T>`, or `IOptionsMonitor<T>` for configuration access.
- Define configuration classes in `Application/Configuration/` or `Api/Configuration/`.
- Use `IOptions<T>` for singleton configurations loaded at startup.
- Use `IOptionsSnapshot<T>` for configurations that can change per request.
- Use `IOptionsMonitor<T>` for configurations that change and need callbacks.
- Validate configuration on startup using `IOptions<T>.Value` in `Program.cs`.
- Name configuration sections clearly (e.g., `JwtSettings`, `DatabaseSettings`).
- Bind configuration using `builder.Configuration.GetSection("SectionName").Get<T>()`.

### DTO Standards (Records for Requests/Responses)

- Use `record` types for all request and response DTOs.
- Prefer positional records for simple DTOs: `public record CreateUserRequest(string Name, string Email);`
- Use immutable records to ensure data integrity during transfer.
- Records provide value-based equality and built-in deconstruction.
- Place DTOs in `Application/DTOs/` or `Api/DTOs/` folders.

### Security Standards

- **Never leak sensitive information** in logs, error messages, or API responses.
- Avoid logging passwords, API keys, tokens, or personal data.
- Use structured logging with sensitive data redaction.
- Return generic error messages to clients (avoid exposing internal system details).
- Implement proper input validation and sanitization.
- Use HTTPS in production environments.

### Additional Guidance

- Keep `Program.cs` minimal; move configuration to `Configuration/`.
- DTOs under `Api/Contracts/V1/...` or `Models/V1/...`.
- EF Core configurations in `Infrastructure`; domain models remain clean.
- Prefer folder + attribute versioning; avoid Areas for APIs.
- Maintain separation between Domain and Infrastructure.

---

## 3. Database Standards (Web API / EF Core)

### Schema / Migrations

- Use EF Core code-first migrations.
- Name migrations clearly (e.g., `AddProduct`, `UpdateCustomerIndexes`).
- Apply migrations consistently across environments.

### Tables & Columns

- PascalCase for table and column names.
- Include standard columns for most tables:
  - `Id` (primary key, `Guid` or `uint`)
  - `ExternalId` (optional, for integration)
  - `CreatedAt` (`DateTimeOffset`, UTC)
  - `UpdatedAt` (`DateTimeOffset`, UTC, nullable)

### Constraints & Indexes

- Enforce PKs, FKs, and unique constraints at the DB level.
- Index frequently queried columns.
- Use EF Core Fluent API for constraints; do not rely solely on annotations.

### Encoding & Collation

- Use UTF8MB4 (MySQL) or equivalent.
- Maintain consistent collation across tables.

### Timestamps

- Store timestamps in UTC.
- `CreatedAt` defaults to current UTC.
- `UpdatedAt` updated automatically on modification.

### Sensitive Data

- Encrypt sensitive fields (e.g., passwords, API keys) at the application level.
- Never store secrets in plaintext.

### Identity & User Management Standards

- Create sealed `ApplicationUser` class that extends `IdentityUser` for custom user properties.
- Use `IdentityDbContext<ApplicationUser>` or inherit from it for user management database context.
- Register Identity services with `builder.Services.AddIdentity<ApplicationUser, IdentityRole>()`.
- Configure Identity stores with `AddEntityFrameworkStores<ApplicationDbContext>()`.
- Use `UserManager<ApplicationUser>` for all user management operations (create, update, delete, password validation).
- Identity tables (AspNetUsers, AspNetRoles, etc.) follow the same naming and migration standards.
- Initially create "Admin" and "Member" roles in the database migration or startup seeding.

### EF Core Configuration

- Entity configurations in `Infrastructure/Persistence/Configurations/`.
- Avoid coupling domain entities directly to EF Core; use shadow properties if needed.
- Configure DbContext with `options.UseMySql()` using Pomelo.EntityFrameworkCore.MySql provider.
- Retrieve connection string from `IConfiguration` using `configuration.GetConnectionString("<db>")`.
- Call `dbContext.Database.Migrate()` on application startup in Development environment only.

### Naming Conventions

- Tables: PascalCase, singular (e.g., `Product`).
- Columns: PascalCase matching property names.
- Foreign keys: `<ReferencedTableName>Id` (e.g., `CustomerId`).

### Migrations & CI/CD

- Track applied migrations in source control.
- Ensure migrations can safely run in CI/CD without data loss.
