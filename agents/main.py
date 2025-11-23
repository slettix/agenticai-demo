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
from models.requests import ProcessGenerationRequest, RevisionRequest
from models.responses import JobResponse, JobStatusResponse
from services.job_queue import JobQueue


# Load environment variables
load_dotenv()

# Initialize job queue
job_queue = JobQueue()

@asynccontextmanager
async def lifespan(app: FastAPI):
    """Application lifespan manager"""
    # Startup
    print("🤖 Starting AI Agents Service...")
    await job_queue.initialize()
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

# Initialize agents
process_generator = ProcessGeneratorAgent()
revision_agent = RevisionAgent()


@app.get("/")
async def root():
    """Health check endpoint"""
    return {
        "service": "ProsessPortal AI Agents",
        "status": "running",
        "version": "1.0.0",
        "agents": ["process_generator", "revision_agent"]
    }


@app.post("/api/agents/generate-process", response_model=JobResponse)
async def generate_process(request: ProcessGenerationRequest):
    """
    Generate a new process using AI
    """
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


if __name__ == "__main__":
    port = int(os.getenv("PORT", 8001))
    uvicorn.run(
        "main:app", 
        host="0.0.0.0", 
        port=port, 
        reload=True,
        log_level="info"
    )