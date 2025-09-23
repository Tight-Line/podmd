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

### EF Core Configuration

- Entity configurations in `Infrastructure/Persistence/Configurations/`.
- Avoid coupling domain entities directly to EF Core; use shadow properties if needed.

### Naming Conventions

- Tables: PascalCase, singular (e.g., `Product`).
- Columns: PascalCase matching property names.
- Foreign keys: `<ReferencedTableName>Id` (e.g., `CustomerId`).

### Migrations & CI/CD

- Track applied migrations in source control.
- Ensure migrations can safely run in CI/CD without data loss.
