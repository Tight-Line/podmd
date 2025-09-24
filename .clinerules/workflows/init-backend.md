# Workflow: Initialize Backend API Project

## Overview

This workflow initializes a .NET backend API project with Clean Architecture, following the established memory-bank standards and code patterns.

## Prerequisites

- Memory bank is initialized and populated
- .NET 9.0 SDK installed
- Development environment configured

## Step 1: Project Setup

1. Look into the **memory-bank** for all available `codeStandards` files.
2. Check **memory-bank** for `techContext` files to identify the defined backend tech stack.
3. Present the backend tech stack from techContext and ask user to confirm if it's correct.
4. Ask **where to create the project** (path/folder).
5. Scaffold the backend project structure according to the confirmed tech stack and code standards.

## Step 2: Configure Database

1. Check the confirmed tech stack for the defined database (from techContext).
2. Confirm with user that the database from techContext is correct, or allow selection from available options in memory bank.
3. Add the **database integration layer** according to the code standards.
4. Ensure environment variables are used for connection strings and credentials.

## Step 3: Core Implementation

1. Implement Clean Architecture layers (Domain, Application, Infrastructure).
2. Set up dependency injection and service registrations.
3. Configure ASP.NET Core pipeline (middleware, routing, CORS).
4. Implement basic health checks and logging.

## Step 4: API Development

1. Create base API controllers with proper routing.
2. [Optional] Ask user if they want to implement authentication and authorization.
   - If yes, check memory bank for authentication patterns and implement according to techContext.md and systemPatterns.md specifications (JWT, ASP.NET Core Identity).
   - Create initial EF Core migration for Identity tables (users, roles, claims).
3. Add request/response models and validation.
4. Configure API versioning and documentation.

## Step 5: Optional Features

- **Health Check**: `/health` endpoint for monitoring
- **API Documentation**: Swagger/OpenAPI documentation
- **Docker Support**: Containerization with Dockerfile
- **Docker Compose**: Orchestration of API and database together
- **Database Migration**: EF Core migrations setup
- **Environment Configuration**: `.env` files and configuration

## Step 6: Testing Setup

1. Create xUnit test projects for each layer.
2. Configure basic test infrastructure and dependencies.

## Step 7: Finalize Backend Setup

1. Generate comprehensive README with setup instructions.
2. Configure launch settings for development.
3. Verify the API starts correctly and health checks pass.
4. Document API endpoints and usage examples.

## Success Criteria

- Backend API builds and runs successfully
- Database connection established and migrations applied
- Health checks return healthy status
- Basic CRUD operations functional
- API documentation accessible
- Authentication and authorization working (if implemented)
