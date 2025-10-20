"""
Environment-based configuration for PodMD application.

Uses Pydantic V2 settings for type-safe environment variable management.
"""

from pydantic import Field
from pydantic_settings import BaseSettings


class Settings(BaseSettings):
    """
    Application settings loaded from environment variables.

    Supports .env file loading with python-dotenv.
    """

    # Database
    database_url: str = Field(
        ...,
        description="PostgreSQL database URL with asyncpg driver",
        json_schema_extra={"examples": ["postgresql+asyncpg://user:password@localhost:5432/podmd"]},
    )

    # Application
    app_port: int = Field(
        default=8080,
        description="Port for the FastAPI application",
        ge=1000,
        le=65535,
    )

    app_host: str = Field(
        default="0.0.0.0",
        description="Host for the FastAPI application",
    )

    # JWT
    jwt_secret_key: str = Field(
        ...,
        description="Secret key for JWT tokens. Must be at least 32 characters.",
        min_length=32,
    )

    access_token_expire_minutes: int = Field(
        default=30,
        description="Expiration time for access tokens in minutes",
        ge=1,
        le=1440,  # Max 24 hours
    )

    class Config:
        """
        Pydantic settings configuration.

        Enables loading from .env files and environment variables.
        """
        env_file = ".env"
        env_file_encoding = "utf-8"
        case_sensitive = False


# Global settings instance
settings = Settings()
