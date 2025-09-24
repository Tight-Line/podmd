# PodMD Backend API

A .NET 9.0 Web API for PodMD - an AI-powered Kubernetes and CI/CD log analysis platform.

## Architecture

This project follows Clean Architecture principles with four layers:

- **PodMD.Api**: ASP.NET Core Web API controllers and configuration
- **PodMD.Application**: Use cases, services, and application logic
- **PodMD.Domain**: Business entities and domain rules
- **PodMD.Infrastructure**: External dependencies and data access

## Technologies

- **Framework**: .NET 9.0 with C# 13
- **Database**: MySQL 8.0 with Entity Framework Core 9.0
- **Authentication**: JWT Bearer tokens with ASP.NET Core Identity
- **Documentation**: Swagger/OpenAPI
- **Logging**: Serilog
- **Containerization**: Docker & Docker Compose

## Prerequisites

- .NET 9.0 SDK
- MySQL 8.0 (or Docker)
- Docker & Docker Compose (optional)

## Quick Start

### Using Docker Compose (Recommended)

1. Clone the repository and navigate to the backend directory
2. Run the application with Docker Compose:

   ```bash
   docker-compose up --build
   ```

3. The API will be available at `http://localhost:8080`
4. Swagger UI at `http://localhost:8080/swagger`

### Local Development

1. Install prerequisites
2. Set up MySQL database:

   ```sql
   CREATE DATABASE PodMD_Dev;
   ```

3. Update connection string in `appsettings.Development.json`
4. Run the application:

   ```bash
   cd src/PodMD.Api
   dotnet run
   ```

5. The API will be available at `https://localhost:5001`
6. Swagger UI at `https://localhost:5001/swagger`

## Configuration

### Environment Variables

| Variable                     | Description             | Default                                   |
| ---------------------------- | ----------------------- | ----------------------------------------- |
| `Database__ConnectionString` | MySQL connection string | `Server=localhost;Database=PodMD_Dev;...` |
| `Jwt__Secret`                | JWT signing key         | Development secret                        |
| `Jwt__Issuer`                | JWT issuer              | `PodMD`                                   |
| `Jwt__Audience`              | JWT audience            | `PodMD`                                   |
| `ASPNETCORE_ENVIRONMENT`     | Environment             | `Development`                             |

### Database Migration

To create and apply migrations:

```bash
cd src/PodMD.Api
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## API Endpoints

### Authentication

- `POST /api/v1/auth/register` - Register new user
- `POST /api/v1/auth/login` - Login user

### Health Check

- `GET /health` - Application health status

## Development

### Project Structure

```
dotnet-backend/
├── src/
│   ├── PodMD.Api/           # Web API project
│   ├── PodMD.Application/   # Application services
│   ├── PodMD.Domain/        # Domain entities
│   ├── PodMD.Infrastructure/# Data access & external services
│   └── PodMD.Shared/        # Shared utilities
├── PodMD.sln               # Solution file
├── docker-compose.yml      # Docker orchestration
├── Directory.Build.props   # Shared build settings
└── Directory.Packages.props # Centralized package versions
```

### Building

```bash
# Build all projects
dotnet build

# Run tests (when implemented)
dotnet test
```

### Code Standards

- Follows Microsoft .NET coding conventions
- Uses nullable reference types
- Implements Clean Architecture patterns
- Uses dependency injection throughout

## Deployment

### Docker

Build and run with Docker:

```bash
docker build -f src/PodMD.Api/Dockerfile -t podmd-api .
docker run -p 8080:80 podmd-api
```

### Production Considerations

- Change JWT secret and database credentials
- Use environment-specific configuration
- Enable HTTPS in production
- Configure proper logging and monitoring
- Set up database backups and migrations

## Contributing

1. Follow the established code standards
2. Write tests for new functionality
3. Update documentation as needed
4. Ensure all builds pass before submitting PRs

## License

[Add license information here]
