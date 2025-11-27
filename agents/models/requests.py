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


class SIAMAnalysisRequest(BaseModel):
    """Request model for SIAM analysis"""
    scenario_description: str = Field(..., description="Description of the multi-vendor scenario to analyze")
    requirements: Optional[List[str]] = Field(default=None, description="Specific requirements or constraints")
    vendor_count: Optional[int] = Field(default=None, description="Number of vendors involved")
    service_complexity: Optional[str] = Field(default="medium", description="Service complexity level: low, medium, high")
    organizational_maturity: Optional[str] = Field(default="medium", description="SIAM maturity level: low, medium, high")
    analysis_type: str = Field(..., description="Type of analysis: scenario, governance, integration, vendor_assessment, sla")
    user_id: str = Field(..., description="ID of the user requesting analysis")
    
    class Config:
        json_schema_extra = {
            "example": {
                "scenario_description": "Implementing multi-vendor IT services with 3 providers: infrastructure, applications, and security",
                "requirements": [
                    "24/7 service availability",
                    "Integrated incident management",
                    "Compliance with Norwegian regulations"
                ],
                "vendor_count": 3,
                "service_complexity": "high",
                "organizational_maturity": "medium",
                "analysis_type": "scenario",
                "user_id": "user_123"
            }
        }


class GovernanceGuidanceRequest(BaseModel):
    """Request model for SIAM governance guidance"""
    vendor_count: int = Field(..., description="Number of vendors to coordinate")
    service_complexity: str = Field(..., description="Service complexity: low, medium, high")
    organizational_maturity: str = Field(..., description="SIAM maturity: low, medium, high")
    industry_sector: Optional[str] = Field(default=None, description="Industry sector (e.g., government, finance, healthcare)")
    specific_challenges: Optional[List[str]] = Field(default=None, description="Specific governance challenges to address")
    user_id: str = Field(..., description="ID of the user requesting guidance")
    
    class Config:
        json_schema_extra = {
            "example": {
                "vendor_count": 4,
                "service_complexity": "high",
                "organizational_maturity": "medium",
                "industry_sector": "government",
                "specific_challenges": [
                    "Complex decision-making processes",
                    "Multiple stakeholder groups",
                    "Regulatory compliance requirements"
                ],
                "user_id": "user_123"
            }
        }


class VendorReadinessRequest(BaseModel):
    """Request model for vendor readiness assessment"""
    vendor_profiles: List[Dict[str, Any]] = Field(..., description="List of vendor profiles to assess")
    assessment_criteria: Optional[List[str]] = Field(default=None, description="Specific criteria for assessment")
    user_id: str = Field(..., description="ID of the user requesting assessment")
    
    class Config:
        json_schema_extra = {
            "example": {
                "vendor_profiles": [
                    {
                        "name": "TechCorp AS",
                        "services": ["Infrastructure", "Cloud"],
                        "itil_maturity": "high",
                        "integration_experience": "medium",
                        "norwegian_presence": True
                    }
                ],
                "assessment_criteria": [
                    "ITIL process maturity",
                    "Integration capabilities",
                    "Norwegian market experience"
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