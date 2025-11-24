"""
Simple FastAPI main application for testing AI Agents connectivity
"""
import time
import uuid
from typing import Dict, Any
from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel

# Simple in-memory job storage for testing
jobs_store: Dict[str, Dict[str, Any]] = {}

class ProcessGenerationRequest(BaseModel):
    title: str
    description: str
    category: str = "ITSM"
    requirements: list = []
    target_audience: str = "IT teams"
    complexity_level: str = "medium"
    user_id: str = "unknown"

class JobResponse(BaseModel):
    job_id: str
    status: str
    message: str

class JobStatusResponse(BaseModel):
    job_id: str
    status: str
    progress: int = 0
    message: str = ""
    error_message: str = None

app = FastAPI(
    title="ProsessPortal AI Agents (Simple)",
    description="Simple AI-powered process generation service for testing",
    version="1.0.0"
)

# CORS configuration for frontend integration
app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://localhost:3000"],  # React frontend
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

@app.get("/")
async def root():
    """Health check endpoint"""
    return {
        "service": "ProsessPortal AI Agents (Simple)",
        "status": "running",
        "version": "1.0.0",
        "agents_available": True,
        "agents": ["process_generator_mock", "revision_agent_mock"],
        "message": "Ready to process AI requests (mock mode)"
    }

@app.post("/api/agents/generate-process", response_model=JobResponse)
async def generate_process(request: ProcessGenerationRequest):
    """
    Generate a new process using mock AI
    """
    job_id = str(uuid.uuid4())
    
    # Store the job
    jobs_store[job_id] = {
        "job_id": job_id,
        "status": "queued",
        "progress": 0,
        "message": "Process generation starting...",
        "request_data": request.dict(),
        "created_at": time.time()
    }
    
    return JobResponse(
        job_id=job_id,
        status="queued",
        message="Process generation job submitted successfully"
    )

@app.get("/api/jobs/{job_id}/status", response_model=JobStatusResponse)
async def get_job_status(job_id: str):
    """
    Get the status of a submitted job
    """
    if job_id not in jobs_store:
        raise HTTPException(status_code=404, detail="Job not found")
    
    job = jobs_store[job_id]
    
    # Simulate progress for testing
    elapsed = time.time() - job["created_at"]
    
    if elapsed < 5:
        progress = min(80, int(elapsed * 16))  # Progress to 80% in 5 seconds
        status = "processing"
        message = "Analyzing requirements and generating process steps..."
    else:
        progress = 100
        status = "completed"
        message = "Process generation completed successfully"
    
    # Update job status
    jobs_store[job_id].update({
        "status": status,
        "progress": progress,
        "message": message
    })
    
    return JobStatusResponse(
        job_id=job_id,
        status=status,
        progress=progress,
        message=message
    )

@app.get("/api/jobs/{job_id}/result")
async def get_job_result(job_id: str):
    """
    Get the result of a completed job
    """
    if job_id not in jobs_store:
        raise HTTPException(status_code=404, detail="Job not found")
    
    job = jobs_store[job_id]
    
    if job["status"] != "completed":
        raise HTTPException(status_code=400, detail="Job not completed yet")
    
    request_data = job["request_data"]
    
    # Generate mock result based on request
    mock_result = {
        "job_id": job_id,
        "title": request_data["title"],
        "description": request_data["description"],
        "category": request_data["category"],
        "tags": ["AI-generert", "ITSM", "mock"],
        "steps": [
            {
                "title": f"Start {request_data['title']}",
                "description": "Initier prosessen og registrer forespørsel",
                "type": 0,  # Start
                "responsible_role": "Service Desk",
                "estimated_duration": 5,
                "order_index": 1,
                "is_optional": False,
                "detailed_instructions": "Registrer incident/forespørsel i ITSM-verktøy og tildel unikt ID"
            },
            {
                "title": "Vurder og klassifiser",
                "description": "Analyser og kategoriser forespørselen",
                "type": 1,  # Task
                "responsible_role": "Service Desk",
                "estimated_duration": 15,
                "order_index": 2,
                "is_optional": False,
                "detailed_instructions": "Klassifiser etter prioritet og kategori basert på ITIL-retningslinjer"
            },
            {
                "title": "Utfør tiltak",
                "description": "Implementer løsning eller eskalere",
                "type": 1,  # Task
                "responsible_role": "Technical Team",
                "estimated_duration": 30,
                "order_index": 3,
                "is_optional": False,
                "detailed_instructions": "Følg etablerte prosedyrer for å løse problemet eller eskaler til riktig team"
            },
            {
                "title": "Verifiser løsning",
                "description": "Kontroller at løsningen fungerer som forventet",
                "type": 6,  # Review
                "responsible_role": "Service Desk",
                "estimated_duration": 10,
                "order_index": 4,
                "is_optional": False,
                "detailed_instructions": "Test løsningen og bekreft med bruker at problemet er løst"
            },
            {
                "title": "Lukk prosess",
                "description": "Dokumenter løsning og lukk saken",
                "type": 8,  # End
                "responsible_role": "Service Desk", 
                "estimated_duration": 5,
                "order_index": 5,
                "is_optional": False,
                "detailed_instructions": "Oppdater kunnskapsdatabase og lukk saken med passende dokumentasjon"
            }
        ],
        "estimated_total_duration": 65,
        "compliance_score": 85,
        "recommendations": [
            "Legg til automasjon for rutinemessige oppgaver",
            "Implementer kvalitetskontroll",
            "Vurder SLA-målsetninger"
        ]
    }
    
    return mock_result

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(
        "simple_main:app", 
        host="0.0.0.0", 
        port=8001, 
        reload=True,
        log_level="info"
    )