"""
PodMD Python Backend - Analysis Schemas
Pydantic models for LLM analysis requests and responses.
"""

from typing import Optional, List, Dict, Any
from pydantic import BaseModel, Field


class Step(BaseModel):
    """Represents a troubleshooting step."""
    title: str
    explanation: str
    command: Optional[str] = None


class Solution(BaseModel):
    """Represents a troubleshooting solution."""
    description: str
    steps: List[Step]


class LogError(BaseModel):
    """Represents an identified error from logs."""
    description: str = Field(..., description="Short description of the error")
    occurrences: List[str] = Field(..., description="List of exact error messages from logs")
    solutions: List[Solution] = Field(..., description="List of troubleshooting solutions")


class AnalysisData(BaseModel):
    """Container for analysis results."""
    errors: List[LogError] = Field(default_factory=list, description="List of identified errors")


class AnalysisResponse(BaseModel):
    """Response model for log analysis."""
    success: bool
    message: str
    data: Optional[Dict[str, Any]] = None


class AnalysisRequest(BaseModel):
    """Request model for log analysis."""
    pass  # Placeholder for future extended request fields
