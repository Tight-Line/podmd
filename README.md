# PodMD

PodMD is a Web API service that automates troubleshooting for failed Kubernetes pods/deployments and CI builds (Jenkins, GitLab). It fetches logs, enriches them with knowledge via Retrieval-Augmented Generation (RAG), and uses Large Language Models (LLMs) to produce structured, actionable "how to fix" recommendations.

## Current Phase

This repository is currently in **Phase 1: Core Application Scaffold**. The minimal FastAPI application with PostgreSQL health checks is fully functional and demonstrates the foundation for the complete PodMD system.

## Quick Start

Get the application running in minutes with Docker Compose:

```bash
# Start the application
docker-compose up --build

# The API will be available at http://localhost:8080
```

## Prerequisites

- Docker and Docker Compose
- 4GB available RAM (minimum)
- Ports 8080 and 5432 available on localhost

## API Endpoints

### Health Check

```bash
GET /health
```

Returns database connectivity status:

```json
{
  "status": "healthy",
  "database": "connected"|"disconnected",
  "timestamp": "ISO8601"
}
```

### API Information

```bash
GET /
```

Returns basic API information.

### API Documentation

```bash
GET /docs
```

Access interactive Swagger UI documentation.

## Success Criteria

```bash
# Health endpoint returns 200 OK with expected JSON
curl http://localhost:8080/health

# Expected: {"status":"healthy","database":"connected","timestamp":"ISO8601"}
```

## Development

### Using Docker Compose

```bash
# Build and start
docker-compose up --build

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

### Local Development

```bash
# Install dependencies
poetry install

# Set up environment
cp .env .env.local

# Run application
poetry run uvicorn app.main:app --reload
```

### Testing

```bash
# Run tests
poetry run pytest
```

## Project Structure

```
podmd/
├── app/
│   ├── main.py           # FastAPI application and routes
│   ├── config.py         # Environment configuration
│   └── database.py       # Database setup
├── pyproject.toml         # Poetry dependencies
├── docker-compose.yml     # Docker orchestration
├── Dockerfile            # API containerization
└── .env                  # Environment template
```
