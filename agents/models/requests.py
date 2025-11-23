"""
Pydantic models for API requests
"""
from typing import Optional, List, Dict, Any
from pydantic import BaseModel, Field


class ProcessGenerationRequest(BaseModel):
    """Request model for process generation"""
    title: str = Field(..., description="Title for the new process")
    description: str = Field(..., description="High-level description of what the process should accomplish")
    category: str = Field(..., description="Process category (e.g., 'HR', 'Finance', 'IT')")
    requirements: Optional[List[str]] = Field(default=None, description="Specific requirements or constraints")
    target_audience: Optional[str] = Field(default=None, description="Who will use this process")
    complexity_level: Optional[str] = Field(default="medium", description="Simple, medium, or complex")
    user_id: str = Field(..., description="ID of the user requesting generation")
    
    class Config:
        json_schema_extra = {
            "example": {
                "title": "Employee Onboarding Process",
                "description": "A comprehensive process for onboarding new employees",
                "category": "HR",
                "requirements": [
                    "Must include IT setup steps",
                    "Should include compliance training",
                    "Must be completed within 5 business days"
                ],
                "target_audience": "HR personnel and managers",
                "complexity_level": "medium",
                "user_id": "user_123"
            }
        }


class RevisionRequest(BaseModel):
    """Request model for process revision"""
    process_id: int = Field(..., description="ID of the process to revise")
    revision_type: str = Field(..., description="Type of revision: 'optimize', 'simplify', 'expand', 'custom'")
    feedback: Optional[List[str]] = Field(default=None, description="Specific feedback to address")
    improvement_goals: Optional[List[str]] = Field(default=None, description="What should be improved")
    custom_instructions: Optional[str] = Field(default=None, description="Custom revision instructions")
    user_id: str = Field(..., description="ID of the user requesting revision")
    
    class Config:
        json_schema_extra = {
            "example": {
                "process_id": 1,
                "revision_type": "optimize",
                "feedback": [
                    "Process takes too long",
                    "Some steps are unclear",
                    "Missing approval workflows"
                ],
                "improvement_goals": [
                    "Reduce total time by 30%",
                    "Improve clarity of instructions",
                    "Add proper approval gates"
                ],
                "user_id": "user_123"
            }
        }


class JobRequest(BaseModel):
    """Base job request model"""
    agent_type: str = Field(..., description="Type of agent to execute the job")
    request_data: Dict[str, Any] = Field(..., description="Request data for the agent")
    user_id: str = Field(..., description="ID of the user submitting the job")
    priority: Optional[int] = Field(default=1, description="Job priority (1=high, 2=medium, 3=low)")
    callback_url: Optional[str] = Field(default=None, description="URL to call when job is complete")