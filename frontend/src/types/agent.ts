export interface ProcessGenerationRequest {
  title: string;
  description: string;
  category: string;
  requirements?: string[];
  targetAudience?: string;
  complexityLevel: 'simple' | 'medium' | 'complex';
}

export interface ProcessRevisionRequest {
  processId: number;
  revisionType: 'optimize' | 'simplify' | 'expand' | 'custom';
  feedback?: string[];
  improvementGoals?: string[];
  customInstructions?: string;
}

export interface AgentJobResponse {
  jobId: string;
  status: string;
  message: string;
  estimatedDuration?: number;
}

export interface AgentJobStatus {
  jobId: string;
  status: 'queued' | 'running' | 'completed' | 'failed';
  progress?: number;
  message?: string;
  createdAt: string;
  startedAt?: string;
  completedAt?: string;
  errorMessage?: string;
}

export interface GeneratedStep {
  title: string;
  description: string;
  type: string;
  responsibleRole?: string;
  estimatedDuration?: number;
  orderIndex: number;
  isOptional?: boolean;
  detailedInstructions?: string;
}

export interface ProcessGenerationResult {
  title: string;
  description: string;
  category: string;
  steps: GeneratedStep[];
  estimatedDuration?: number;
  tags?: string[];
  metadata?: Record<string, any>;
}

export interface ProcessRevisionResult {
  processId: number;
  revisionSummary: string;
  updatedTitle?: string;
  updatedDescription?: string;
  updatedSteps?: GeneratedStep[];
  changesMade: string[];
  improvementMetrics?: Record<string, any>;
  metadata?: Record<string, any>;
}

export interface AgentServiceHealth {
  agentServiceAvailable: boolean;
  status: 'healthy' | 'degraded' | 'unhealthy';
  message: string;
}