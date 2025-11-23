"""
Pydantic models for API responses
"""
from typing import Optional, Any, Dict
from datetime import datetime
from pydantic import BaseModel, Field


class JobResponse(BaseModel):
    """Response model for job submission"""
    job_id: str = Field(..., description="Unique job identifier")
    status: str = Field(..., description="Current job status")
    message: str = Field(..., description="Human-readable status message")
    estimated_duration: Optional[int] = Field(default=None, description="Estimated duration in seconds")
    
    class Config:
        json_schema_extra = {
            "example": {
                "job_id": "job_abc123",
                "status": "queued",
                "message": "Process generation job submitted successfully",
                "estimated_duration": 120
            }
        }


class JobStatusResponse(BaseModel):
    """Response model for job status queries"""
    job_id: str = Field(..., description="Unique job identifier")
    status: str = Field(..., description="Current job status: queued, running, completed, failed")
    progress: Optional[int] = Field(default=None, description="Progress percentage (0-100)")
    message: Optional[str] = Field(default=None, description="Current status message")
    created_at: datetime = Field(..., description="Job creation timestamp")
    started_at: Optional[datetime] = Field(default=None, description="Job start timestamp")
    completed_at: Optional[datetime] = Field(default=None, description="Job completion timestamp")
    error_message: Optional[str] = Field(default=None, description="Error message if job failed")
    
    class Config:
        json_schema_extra = {
            "example": {
                "job_id": "job_abc123",
                "status": "running",
                "progress": 65,
                "message": "Generating process steps...",
                "created_at": "2024-01-15T10:30:00Z",
                "started_at": "2024-01-15T10:30:05Z",
                "completed_at": None,
                "error_message": None
            }
        }


class ProcessGenerationResult(BaseModel):
    """Result model for process generation"""
    title: str = Field(..., description="Generated process title")
    description: str = Field(..., description="Generated process description")
    category: str = Field(..., description="Process category")
    steps: list[Dict[str, Any]] = Field(..., description="Generated process steps")
    estimated_duration: Optional[int] = Field(default=None, description="Estimated process duration in minutes")
    tags: Optional[list[str]] = Field(default=None, description="Suggested tags for the process")
    metadata: Optional[Dict[str, Any]] = Field(default=None, description="Additional metadata")
    
    class Config:
        json_schema_extra = {
            "example": {
                "title": "Employee Onboarding Process",
                "description": "Comprehensive process for onboarding new employees",
                "category": "HR",
                "steps": [
                    {
                        "title": "Prepare Workstation",
                        "description": "Set up computer and workspace for new employee",
                        "order_index": 1,
                        "type": "Task",
                        "estimated_duration": 30,
                        "responsible_role": "IT"
                    }
                ],
                "estimated_duration": 240,
                "tags": ["onboarding", "hr", "new-employee"],
                "metadata": {
                    "ai_generated": True,
                    "generation_model": "gpt-4",
                    "confidence_score": 0.95
                }
            }
        }


class RevisionResult(BaseModel):
    """Result model for process revision"""
    process_id: int = Field(..., description="ID of the revised process")
    revision_summary: str = Field(..., description="Summary of changes made")
    updated_title: Optional[str] = Field(default=None, description="Updated process title")
    updated_description: Optional[str] = Field(default=None, description="Updated process description")
    updated_steps: Optional[list[Dict[str, Any]]] = Field(default=None, description="Updated process steps")
    changes_made: list[str] = Field(..., description="List of specific changes made")
    improvement_metrics: Optional[Dict[str, Any]] = Field(default=None, description="Metrics on improvements")
    metadata: Optional[Dict[str, Any]] = Field(default=None, description="Additional metadata")
    
    class Config:
        json_schema_extra = {
            "example": {
                "process_id": 1,
                "revision_summary": "Optimized process by removing redundant steps and clarifying instructions",
                "updated_title": "Streamlined Employee Onboarding Process",
                "updated_description": "Efficient and clear process for onboarding new employees",
                "changes_made": [
                    "Removed duplicate approval step",
                    "Clarified IT setup requirements",
                    "Added parallel processing for forms"
                ],
                "improvement_metrics": {
                    "estimated_time_saved": 60,
                    "steps_reduced": 2,
                    "clarity_score": 0.92
                },
                "metadata": {
                    "ai_revised": True,
                    "revision_model": "gpt-4",
                    "confidence_score": 0.88
                }
            }
        }