import { authService } from './authService.ts';
import type { 
  ProcessGenerationRequest, 
  ProcessRevisionRequest, 
  AgentJobResponse, 
  AgentJobStatus,
  ProcessGenerationResult,
  ProcessRevisionResult,
  AgentServiceHealth
} from '../types/agent.ts';

const API_URL = 'http://localhost:5001/api';

class AgentService {
  private async request<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
    const token = authService.getToken();
    
    const response = await fetch(`${API_URL}${endpoint}`, {
      ...options,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': token ? `Bearer ${token}` : '',
        ...options.headers,
      },
    });

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}));
      throw new Error(errorData.error || errorData.Error || `HTTP ${response.status}`);
    }

    return response.json();
  }

  async getHealth(): Promise<AgentServiceHealth> {
    return this.request<AgentServiceHealth>('/agent/health');
  }

  async generateProcess(request: ProcessGenerationRequest): Promise<AgentJobResponse> {
    return this.request<AgentJobResponse>('/agent/generate-process', {
      method: 'POST',
      body: JSON.stringify(request),
    });
  }

  async reviseProcess(request: ProcessRevisionRequest): Promise<AgentJobResponse> {
    return this.request<AgentJobResponse>('/agent/revise-process', {
      method: 'POST',
      body: JSON.stringify(request),
    });
  }

  async getJobStatus(jobId: string): Promise<AgentJobStatus> {
    return this.request<AgentJobStatus>(`/agent/jobs/${jobId}/status`);
  }

  async getGenerationResult(jobId: string): Promise<ProcessGenerationResult> {
    return this.request<ProcessGenerationResult>(`/agent/jobs/${jobId}/result/generation`);
  }

  async getRevisionResult(jobId: string): Promise<ProcessRevisionResult> {
    return this.request<ProcessRevisionResult>(`/agent/jobs/${jobId}/result/revision`);
  }

  async createProcessFromGeneration(jobId: string): Promise<any> {
    return this.request(`/agent/jobs/${jobId}/create-process`, {
      method: 'POST',
    });
  }

  async applyRevision(jobId: string): Promise<any> {
    return this.request(`/agent/jobs/${jobId}/apply-revision`, {
      method: 'POST',
    });
  }

  // Utility method to poll job status until completion
  async pollJobStatus(
    jobId: string, 
    onProgress: (status: AgentJobStatus) => void,
    intervalMs: number = 2000
  ): Promise<AgentJobStatus> {
    return new Promise((resolve, reject) => {
      const poll = async () => {
        try {
          const status = await this.getJobStatus(jobId);
          onProgress(status);

          if (status.status === 'completed' || status.status === 'failed') {
            resolve(status);
          } else {
            setTimeout(poll, intervalMs);
          }
        } catch (error) {
          reject(error);
        }
      };

      poll();
    });
  }
}

export const agentService = new AgentService();