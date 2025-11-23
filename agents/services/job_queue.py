"""
Simple in-memory job queue for AI agent tasks
In production, this would be replaced with Redis or similar
"""
import asyncio
import uuid
from datetime import datetime
from typing import Dict, Any, Optional
from enum import Enum


class JobStatus(str, Enum):
    QUEUED = "queued"
    RUNNING = "running" 
    COMPLETED = "completed"
    FAILED = "failed"


class Job:
    def __init__(self, job_id: str, agent_type: str, request_data: Dict[str, Any], user_id: str):
        self.job_id = job_id
        self.agent_type = agent_type
        self.request_data = request_data
        self.user_id = user_id
        self.status = JobStatus.QUEUED
        self.progress = 0
        self.message = "Job queued"
        self.created_at = datetime.utcnow()
        self.started_at: Optional[datetime] = None
        self.completed_at: Optional[datetime] = None
        self.error_message: Optional[str] = None
        self.result: Optional[Dict[str, Any]] = None


class JobQueue:
    def __init__(self):
        self.jobs: Dict[str, Job] = {}
        self.queue = asyncio.Queue()
        self.worker_task: Optional[asyncio.Task] = None
        self.is_running = False

    async def initialize(self):
        """Initialize the job queue and start worker"""
        self.is_running = True
        self.worker_task = asyncio.create_task(self._worker())
        print("✅ Job queue initialized and worker started")

    async def cleanup(self):
        """Cleanup the job queue"""
        self.is_running = False
        if self.worker_task:
            self.worker_task.cancel()
            try:
                await self.worker_task
            except asyncio.CancelledError:
                pass
        print("✅ Job queue cleaned up")

    async def submit_job(self, agent_type: str, request_data: Dict[str, Any], user_id: str) -> str:
        """Submit a new job to the queue"""
        job_id = f"job_{uuid.uuid4().hex[:8]}"
        job = Job(job_id, agent_type, request_data, user_id)
        
        self.jobs[job_id] = job
        await self.queue.put(job)
        
        print(f"📝 Job {job_id} submitted for agent type: {agent_type}")
        return job_id

    async def get_job_status(self, job_id: str) -> Optional[Dict[str, Any]]:
        """Get the status of a job"""
        job = self.jobs.get(job_id)
        if not job:
            return None

        return {
            "job_id": job.job_id,
            "status": job.status,
            "progress": job.progress,
            "message": job.message,
            "created_at": job.created_at,
            "started_at": job.started_at,
            "completed_at": job.completed_at,
            "error_message": job.error_message
        }

    async def get_job_result(self, job_id: str) -> Optional[Dict[str, Any]]:
        """Get the result of a completed job"""
        job = self.jobs.get(job_id)
        if not job or job.status != JobStatus.COMPLETED:
            return None

        return job.result

    async def _worker(self):
        """Worker coroutine that processes jobs from the queue"""
        from agents.process_generator import ProcessGeneratorAgent
        from agents.revision_agent import RevisionAgent
        
        # Initialize agents
        process_generator = ProcessGeneratorAgent()
        revision_agent = RevisionAgent()
        
        agents = {
            "process_generator": process_generator,
            "revision_agent": revision_agent
        }

        print("🤖 Job queue worker started")
        
        while self.is_running:
            try:
                # Wait for a job with timeout to allow graceful shutdown
                job = await asyncio.wait_for(self.queue.get(), timeout=1.0)
                
                print(f"🔄 Processing job {job.job_id} with agent {job.agent_type}")
                
                # Update job status
                job.status = JobStatus.RUNNING
                job.started_at = datetime.utcnow()
                job.message = "Processing..."
                
                try:
                    # Get the appropriate agent
                    agent = agents.get(job.agent_type)
                    if not agent:
                        raise ValueError(f"Unknown agent type: {job.agent_type}")
                    
                    # Process the job
                    if job.agent_type == "process_generator":
                        result = await agent.generate_process(job.request_data, self._update_job_progress(job))
                    elif job.agent_type == "revision_agent":
                        result = await agent.revise_process(job.request_data, self._update_job_progress(job))
                    else:
                        raise ValueError(f"Unsupported agent type: {job.agent_type}")
                    
                    # Job completed successfully
                    job.status = JobStatus.COMPLETED
                    job.completed_at = datetime.utcnow()
                    job.progress = 100
                    job.message = "Job completed successfully"
                    job.result = result
                    
                    print(f"✅ Job {job.job_id} completed successfully")
                    
                except Exception as e:
                    # Job failed
                    job.status = JobStatus.FAILED
                    job.completed_at = datetime.utcnow()
                    job.error_message = str(e)
                    job.message = f"Job failed: {str(e)}"
                    
                    print(f"❌ Job {job.job_id} failed: {str(e)}")
                
                # Mark task done
                self.queue.task_done()
                
            except asyncio.TimeoutError:
                # No job available, continue loop
                continue
            except asyncio.CancelledError:
                print("🛑 Job queue worker cancelled")
                break
            except Exception as e:
                print(f"❌ Unexpected error in job queue worker: {e}")
                continue

    def _update_job_progress(self, job: Job):
        """Create a progress update callback for a job"""
        async def update_progress(progress: int, message: str = None):
            job.progress = min(max(progress, 0), 100)
            if message:
                job.message = message
            print(f"📊 Job {job.job_id} progress: {progress}% - {message or job.message}")
        
        return update_progress