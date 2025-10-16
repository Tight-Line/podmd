# PodMD Python Backend

Experimental FastAPI implementation of PodMD backend services using Python and PostgreSQL.

## Overview

This is an alternative implementation of the PodMD backend using:

- **FastAPI** for the web framework
- **PostgreSQL** instead of MySQL
- **Python 3.11+** with async/await patterns
- **Docker** for containerization

## Features

✅ Health check endpoint (`/health`)  
✅ FastAPI automatic API documentation (`/docs`)  
✅ PostgreSQL database setup  
✅ Docker containerization  
✅ Environment-based configuration  
⏳ JWT authentication (planned)  
⏳ Kubernetes integration (planned)  
⏳ LLM analysis service (planned)

## Quick Start

### Prerequisites

- Docker and Docker Compose
- Python 3.11+ (optional, for local development)

### Running with Docker Compose

1. **Clone and navigate to the python backend:**

   ```bash
   cd python-backend
   ```

2. **Start the services:**

   ```bash
   docker-compose up -d
   ```

3. **Check the health endpoint:**

   ```bash
   curl http://localhost:8081/health
   ```

   Expected response:

   ```json
   {
     "status": "healthy",
     "service": "podmd-python-backend",
     "timestamp": "2025-10-15T20:36:00.000Z",
     "version": "0.1.0",
     "database": "not_configured"
   }
   ```

4. **View API documentation:**
   Open `http://localhost:8081/docs` in your browser

### Running Locally (Development)

1. **Install dependencies:**

   ```bash
   pip install -r requirements.txt
   ```

2. **Set environment variables:**

   ```bash
   cp .env.example .env
   # Edit .env with your local settings
   ```

3. **Start the development server:**
   ```bash
   uvicorn app.main:app --reload --host 0.0.0.0 --port 8081
   ```

## Architecture

```
python-backend/
├── app/
│   ├── __init__.py       # Package initialization
│   └── main.py           # FastAPI application and routes
├── docker-compose.yml    # Docker orchestration
├── Dockerfile           # Container build instructions
├── requirements.txt     # Python dependencies
├── .env                # Environment configuration
├── .env.example        # Environment template
└── README.md          # This file
```

## Environment Variables

| Variable            | Description                          | Default                |
| ------------------- | ------------------------------------ | ---------------------- |
| `APP_NAME`          | Application name                     | `podmd-python-backend` |
| `APP_ENV`           | Environment (development/production) | `development`          |
| `POSTGRES_HOST`     | PostgreSQL host                      | `postgres`             |
| `POSTGRES_DB`       | Database name                        | `podmd_db`             |
| `POSTGRES_USER`     | Database user                        | `podmd_user`           |
| `POSTGRES_PASSWORD` | Database password                    | (required)             |

## API Endpoints

| Method | Endpoint  | Description                   |
| ------ | --------- | ----------------------------- |
| GET    | `/`       | Root endpoint with API info   |
| GET    | `/health` | Health check                  |
| GET    | `/docs`   | Interactive API documentation |

## Database

PostgreSQL is used instead of MySQL for advanced JSON capabilities and better async performance. The database will be automatically created when you start the services.

## Development

### Project Structure

- **Controllers/Routes**: `app/main.py` (single file for now)
- **Configuration**: Environment variables via `python-dotenv`
- **Dependencies**: Managed via `requirements.txt`
- **Containerization**: Multi-stage Docker builds

### Adding New Features

1. Add route handlers to `app/main.py`
2. Update requirements if new dependencies needed
3. Add environment variables to `.env.example`
4. Update documentation and tests

## Comparison with .NET Backend

| Aspect    | .NET Backend     | Python Backend |
| --------- | ---------------- | -------------- |
| Framework | ASP.NET Core     | FastAPI        |
| Database  | MySQL            | PostgreSQL     |
| Language  | C#               | Python         |
| Port      | 8080             | 8081           |
| Status    | Production Ready | Experimental   |

## Next Steps

- [ ] Database models and migrations
- [ ] Authentication system
- [ ] Kubernetes cluster management
- [ ] LLM integration for log analysis
- [ ] RESTful API endpoints
- [ ] Frontend integration

## Contributing

This is an experimental implementation. Please coordinate with the main .NET backend development for architectural consistency.
