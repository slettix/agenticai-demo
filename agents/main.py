"""
FastAPI main application for AI Agents
"""
import os
from contextlib import asynccontextmanager
from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
import uvicorn
from dotenv import load_dotenv

from agents.process_generator import ProcessGeneratorAgent
from agents.revision_agent import RevisionAgent
from agents.document_classifier import DocumentClassifierAgent
from agents.process_optimizer import ProcessOptimizerAgent
from agents.siam_specialist import SIAMSpecialistAgent
from models.requests import (
    ProcessGenerationRequest, 
    RevisionRequest, 
    SIAMAnalysisRequest, 
    GovernanceGuidanceRequest, 
    VendorReadinessRequest
)
from models.responses import JobResponse, JobStatusResponse
from services.job_queue import JobQueue


# Load environment variables
load_dotenv()

# Initialize job queue
job_queue = JobQueue()

@asynccontextmanager
async def lifespan(app: FastAPI):
    """Application lifespan manager"""
    global process_generator, revision_agent, document_classifier, process_optimizer, siam_specialist
    
    # Startup
    print("🤖 Starting AI Agents Service...")
    
    try:
        # Initialize agents
        from agents.process_generator import ProcessGeneratorAgent
        from agents.revision_agent import RevisionAgent
        from agents.document_classifier import DocumentClassifierAgent
        from agents.process_optimizer import ProcessOptimizerAgent
        from agents.siam_specialist import SIAMSpecialistAgent
        
        process_generator = ProcessGeneratorAgent()
        revision_agent = RevisionAgent()
        document_classifier = DocumentClassifierAgent()
        process_optimizer = ProcessOptimizerAgent()
        siam_specialist = SIAMSpecialistAgent()
        print("✅ AI Agents initialized successfully")
        
        await job_queue.initialize()
        print("✅ Job queue initialized")
        
    except Exception as e:
        print(f"❌ Failed to initialize AI Agents: {e}")
        print("⚠️  AI Agents will not be available")
    
    yield
    
    # Shutdown
    print("🤖 Shutting down AI Agents Service...")
    await job_queue.cleanup()

app = FastAPI(
    title="ProsessPortal AI Agents",
    description="AI-powered process generation and revision agents",
    version="1.0.0",
    lifespan=lifespan
)

# CORS configuration for frontend integration
app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://localhost:3000"],  # React frontend
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Initialize agents (will be created on startup)
process_generator = None
revision_agent = None
document_classifier = None
process_optimizer = None
siam_specialist = None


@app.get("/")
async def root():
    """Health check endpoint"""
    agents_available = all([
        process_generator is not None,
        revision_agent is not None,
        document_classifier is not None,
        process_optimizer is not None,
        siam_specialist is not None
    ])
    
    available_agents = []
    if process_generator: available_agents.append("process_generator")
    if revision_agent: available_agents.append("revision_agent")
    if document_classifier: available_agents.append("document_classifier")
    if process_optimizer: available_agents.append("process_optimizer")
    if siam_specialist: available_agents.append("siam_specialist")
    
    return {
        "service": "ProsessPortal AI Agents",
        "status": "running",
        "version": "1.0.0",
        "agents_available": agents_available,
        "agents": available_agents,
        "epic3_features": ["document_classification", "process_optimization"],
        "epic7_features": ["siam_analysis", "multi_vendor_governance", "vendor_readiness"],
        "message": "Ready to process AI requests" if agents_available else "Some agents not initialized - check OpenAI API key"
    }


@app.post("/api/agents/generate-process", response_model=JobResponse)
async def generate_process(request: ProcessGenerationRequest):
    """
    Generate a new process using AI
    """
    if not process_generator:
        raise HTTPException(status_code=503, detail="AI Agents are not available. Please check OpenAI API key configuration.")
    
    try:
        job_id = await job_queue.submit_job(
            agent_type="process_generator",
            request_data=request.model_dump(),
            user_id=request.user_id
        )
        
        return JobResponse(
            job_id=job_id,
            status="queued",
            message="Process generation job submitted successfully"
        )
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Failed to submit job: {str(e)}")


@app.post("/api/agents/revise-process", response_model=JobResponse)
async def revise_process(request: RevisionRequest):
    """
    Revise an existing process using AI
    """
    if not revision_agent:
        raise HTTPException(status_code=503, detail="AI Agents are not available. Please check OpenAI API key configuration.")
    
    try:
        job_id = await job_queue.submit_job(
            agent_type="revision_agent",
            request_data=request.model_dump(),
            user_id=request.user_id
        )
        
        return JobResponse(
            job_id=job_id,
            status="queued", 
            message="Process revision job submitted successfully"
        )
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Failed to submit job: {str(e)}")


@app.get("/api/jobs/{job_id}/status", response_model=JobStatusResponse)
async def get_job_status(job_id: str):
    """
    Get the status of a submitted job
    """
    try:
        job_status = await job_queue.get_job_status(job_id)
        if not job_status:
            raise HTTPException(status_code=404, detail="Job not found")
        
        return JobStatusResponse(**job_status)
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Failed to get job status: {str(e)}")


@app.get("/api/jobs/{job_id}/result")
async def get_job_result(job_id: str):
    """
    Get the result of a completed job
    """
    try:
        result = await job_queue.get_job_result(job_id)
        if not result:
            raise HTTPException(status_code=404, detail="Job result not found")
        
        return result
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Failed to get job result: {str(e)}")


# Epic 3: AI-driven Process Automation Endpoints

@app.post("/api/agents/classify-document", response_model=JobResponse)
async def classify_document(request: dict):
    """
    Classify a document automatically (Epic 3 - Story 3.1)
    """
    if not document_classifier:
        raise HTTPException(status_code=503, detail="Document classification agent is not available.")
    
    try:
        job_id = await job_queue.submit_job(
            agent_type="document_classifier",
            request_data=request,
            user_id=request.get("user_id", "unknown")
        )
        
        return JobResponse(
            job_id=job_id,
            status="queued",
            message="Document classification job submitted successfully"
        )
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Failed to submit document classification job: {str(e)}")


@app.post("/api/agents/optimize-process", response_model=JobResponse)
async def optimize_process(request: dict):
    """
    Analyze process and provide optimization recommendations (Epic 3 - Story 3.2)
    """
    if not process_optimizer:
        raise HTTPException(status_code=503, detail="Process optimization agent is not available.")
    
    try:
        job_id = await job_queue.submit_job(
            agent_type="process_optimizer",
            request_data=request,
            user_id=request.get("user_id", "unknown")
        )
        
        return JobResponse(
            job_id=job_id,
            status="queued",
            message="Process optimization job submitted successfully"
        )
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Failed to submit process optimization job: {str(e)}")


# Epic 7: SIAM and Multi-Vendor Support Endpoints

@app.post("/api/agents/siam-analysis", response_model=JobResponse)
async def siam_analysis(request: SIAMAnalysisRequest):
    """
    Perform SIAM analysis for multi-vendor scenarios (Epic 7 - Issue #30)
    """
    if not siam_specialist:
        raise HTTPException(status_code=503, detail="SIAM specialist agent is not available.")
    
    try:
        job_id = await job_queue.submit_job(
            agent_type="siam_specialist",
            request_data=request.model_dump(),
            user_id=request.user_id
        )
        
        return JobResponse(
            job_id=job_id,
            status="queued",
            message="SIAM analysis job submitted successfully"
        )
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Failed to submit SIAM analysis job: {str(e)}")


@app.post("/api/agents/governance-guidance", response_model=JobResponse)
async def governance_guidance(request: GovernanceGuidanceRequest):
    """
    Get SIAM governance guidance for multi-vendor environments (Epic 7)
    """
    if not siam_specialist:
        raise HTTPException(status_code=503, detail="SIAM specialist agent is not available.")
    
    try:
        job_id = await job_queue.submit_job(
            agent_type="siam_specialist",
            request_data={**request.model_dump(), "analysis_type": "governance"},
            user_id=request.user_id
        )
        
        return JobResponse(
            job_id=job_id,
            status="queued",
            message="SIAM governance guidance job submitted successfully"
        )
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Failed to submit governance guidance job: {str(e)}")


@app.post("/api/agents/vendor-readiness", response_model=JobResponse)
async def vendor_readiness(request: VendorReadinessRequest):
    """
    Assess vendor readiness for SIAM implementation (Epic 7)
    """
    if not siam_specialist:
        raise HTTPException(status_code=503, detail="SIAM specialist agent is not available.")
    
    try:
        job_id = await job_queue.submit_job(
            agent_type="siam_specialist",
            request_data={**request.model_dump(), "analysis_type": "vendor_assessment"},
            user_id=request.user_id
        )
        
        return JobResponse(
            job_id=job_id,
            status="queued",
            message="Vendor readiness assessment job submitted successfully"
        )
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Failed to submit vendor readiness job: {str(e)}")


@app.get("/api/agents/siam/framework")
async def get_siam_framework():
    """
    Get SIAM framework information (Epic 7)
    """
    if not siam_specialist:
        raise HTTPException(status_code=503, detail="SIAM specialist agent is not available.")
    
    try:
        framework = siam_specialist.get_siam_framework()
        scenarios = siam_specialist.get_multi_vendor_scenarios()
        
        return {
            "framework": framework,
            "multi_vendor_scenarios": scenarios,
            "specializations": siam_specialist.specializations,
            "message": "SIAM framework data retrieved successfully"
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Failed to retrieve SIAM framework: {str(e)}")


@app.get("/api/agents/epic3/features")
async def get_epic3_features():
    """
    Get available Epic 3 (AI-driven Process Automation) features
    """
    return {
        "epic": "Epic 3: AI-driven Process Automation",
        "features": [
            {
                "name": "Document Classification",
                "story": "Story 3.1",
                "description": "Automatic document classification and process suggestion",
                "endpoint": "/api/agents/classify-document",
                "available": document_classifier is not None
            },
            {
                "name": "Process Optimization", 
                "story": "Story 3.2",
                "description": "AI-driven process optimization recommendations",
                "endpoint": "/api/agents/optimize-process",
                "available": process_optimizer is not None
            }
        ],
        "overall_status": "available" if document_classifier and process_optimizer else "partial"
    }


@app.get("/api/agents/epic7/features")
async def get_epic7_features():
    """
    Get available Epic 7 (SIAM and Multi-Vendor Support) features
    """
    return {
        "epic": "Epic 7: SIAM and Multi-Vendor Support",
        "features": [
            {
                "name": "SIAM Analysis",
                "story": "Issue #30",
                "description": "AI-powered SIAM scenario analysis and recommendations",
                "endpoint": "/api/agents/siam-analysis",
                "available": siam_specialist is not None
            },
            {
                "name": "Governance Guidance",
                "story": "Issue #30",
                "description": "Multi-vendor governance structure recommendations",
                "endpoint": "/api/agents/governance-guidance", 
                "available": siam_specialist is not None
            },
            {
                "name": "Vendor Readiness Assessment",
                "story": "Issue #30",
                "description": "Assess vendor readiness for SIAM implementation",
                "endpoint": "/api/agents/vendor-readiness",
                "available": siam_specialist is not None
            },
            {
                "name": "SIAM Framework Access",
                "story": "Issue #30", 
                "description": "Access to SIAM framework and best practices",
                "endpoint": "/api/agents/siam/framework",
                "available": siam_specialist is not None
            }
        ],
        "overall_status": "available" if siam_specialist else "not_available"
    }


if __name__ == "__main__":
    port = int(os.getenv("PORT", 8001))
    uvicorn.run(
        "main:app", 
        host="0.0.0.0", 
        port=port, 
        reload=True,
        log_level="info"
    )