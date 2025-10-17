# Multi-stage Docker build for PodMD using UV for fast Python packaging

# Builder stage for dependency installation
FROM python:3.11-alpine AS builder

# Install uv for fast Python package management
COPY --from=ghcr.io/astral-sh/uv:latest /uv /bin/uv

# Set working directory
WORKDIR /app

# Copy dependency files
COPY pyproject.toml ./

# Install dependencies without dev dependencies for production
RUN uv pip install --system --no-cache-dir -r pyproject.toml

# Runtime stage
FROM python:3.11-alpine AS runtime

# Install curl for health checks
RUN apk add --no-cache curl

# Create non-root user
RUN addgroup -g 1001 -S podmd && \
    adduser -S podmd -u 1001 -G podmd

# Set working directory
WORKDIR /app

# Copy installed packages from builder stage
COPY --from=builder /usr/local/lib/python3.11/site-packages /usr/local/lib/python3.11/site-packages
COPY --from=builder /usr/local/bin /usr/local/bin

# Copy application code
COPY app/ ./app/

# Copy environment file (template, override with actual values in docker-compose)
COPY .env ./

# Change ownership of the app directory
RUN chown -R podmd:podmd /app

# Switch to non-root user
USER podmd

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Run the application
CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8080"]
