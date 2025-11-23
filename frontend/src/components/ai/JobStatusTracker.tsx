import React, { useState, useEffect, useCallback } from 'react';
import { AgentJobStatus, ProcessGenerationResult, ProcessRevisionResult } from '../../types/agent.ts';
import { agentService } from '../../services/agentService.ts';

interface JobStatusTrackerProps {
  jobId: string;
  jobType: 'generation' | 'revision';
  onJobCompleted: (result: ProcessGenerationResult | ProcessRevisionResult) => void;
  onJobFailed: (error: string) => void;
  onCreateProcess?: (jobId: string) => void;
  onApplyRevision?: (jobId: string) => void;
}

export const JobStatusTracker: React.FC<JobStatusTrackerProps> = ({
  jobId,
  jobType,
  onJobCompleted,
  onJobFailed,
  onCreateProcess,
  onApplyRevision
}) => {
  const [status, setStatus] = useState<AgentJobStatus | null>(null);
  const [result, setResult] = useState<ProcessGenerationResult | ProcessRevisionResult | null>(null);
  const [error, setError] = useState<string>('');
  const [isCreating, setIsCreating] = useState(false);
  const [isApplying, setIsApplying] = useState(false);

  const handleStatusUpdate = useCallback((newStatus: AgentJobStatus) => {
    setStatus(newStatus);
    
    if (newStatus.status === 'failed') {
      const errorMsg = newStatus.errorMessage || 'Job failed';
      setError(errorMsg);
      onJobFailed(errorMsg);
    } else if (newStatus.status === 'completed') {
      // Fetch the result
      const fetchResult = async () => {
        try {
          let jobResult;
          if (jobType === 'generation') {
            jobResult = await agentService.getGenerationResult(jobId);
          } else {
            jobResult = await agentService.getRevisionResult(jobId);
          }
          setResult(jobResult);
          onJobCompleted(jobResult);
        } catch (err) {
          const errorMsg = err instanceof Error ? err.message : 'Failed to fetch result';
          setError(errorMsg);
          onJobFailed(errorMsg);
        }
      };
      
      fetchResult();
    }
  }, [jobId, jobType, onJobCompleted, onJobFailed]);

  useEffect(() => {
    if (!jobId) return;

    // Start polling for job status
    agentService.pollJobStatus(jobId, handleStatusUpdate)
      .catch((err) => {
        const errorMsg = err instanceof Error ? err.message : 'Failed to track job status';
        setError(errorMsg);
        onJobFailed(errorMsg);
      });
  }, [jobId, handleStatusUpdate, onJobFailed]);

  const handleCreateProcess = async () => {
    if (!onCreateProcess) return;
    
    setIsCreating(true);
    try {
      await onCreateProcess(jobId);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create process');
    } finally {
      setIsCreating(false);
    }
  };

  const handleApplyRevision = async () => {
    if (!onApplyRevision) return;
    
    setIsApplying(true);
    try {
      await onApplyRevision(jobId);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to apply revision');
    } finally {
      setIsApplying(false);
    }
  };

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'queued': return '⏳';
      case 'running': return '🔄';
      case 'completed': return '✅';
      case 'failed': return '❌';
      default: return '❓';
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'queued': return 'orange';
      case 'running': return 'blue';
      case 'completed': return 'green';
      case 'failed': return 'red';
      default: return 'gray';
    }
  };

  if (!status) {
    return (
      <div className="job-tracker">
        <div className="status-item">
          <span className="status-icon">🔍</span>
          <span>Henter jobbstatus...</span>
        </div>
      </div>
    );
  }

  return (
    <div className="job-tracker">
      <div className="job-header">
        <h3>
          {jobType === 'generation' ? '🤖 AI Prosessgenerering' : '🔧 AI Prosessrevisjon'}
        </h3>
        <div className="job-id">Jobb ID: {jobId}</div>
      </div>

      <div className="status-item" style={{ borderLeft: `4px solid ${getStatusColor(status.status)}` }}>
        <span className="status-icon">{getStatusIcon(status.status)}</span>
        <div className="status-content">
          <div className="status-text">
            <strong>Status:</strong> {status.status}
          </div>
          {status.message && (
            <div className="status-message">{status.message}</div>
          )}
          {status.progress !== undefined && status.status === 'running' && (
            <div className="progress-bar">
              <div 
                className="progress-fill" 
                style={{ width: `${status.progress}%` }}
              />
              <span className="progress-text">{status.progress}%</span>
            </div>
          )}
        </div>
      </div>

      <div className="timing-info">
        <div><strong>Opprettet:</strong> {new Date(status.createdAt).toLocaleString('no-NO')}</div>
        {status.startedAt && (
          <div><strong>Startet:</strong> {new Date(status.startedAt).toLocaleString('no-NO')}</div>
        )}
        {status.completedAt && (
          <div><strong>Ferdig:</strong> {new Date(status.completedAt).toLocaleString('no-NO')}</div>
        )}
      </div>

      {error && (
        <div className="error-message">
          <strong>❌ Feil:</strong> {error}
        </div>
      )}

      {result && status.status === 'completed' && (
        <div className="result-preview">
          <h4>📋 Resultat</h4>
          
          {jobType === 'generation' && (
            <div className="generation-result">
              <div><strong>Tittel:</strong> {(result as ProcessGenerationResult).title}</div>
              <div><strong>Beskrivelse:</strong> {(result as ProcessGenerationResult).description}</div>
              <div><strong>Antall steg:</strong> {(result as ProcessGenerationResult).steps?.length || 0}</div>
              {(result as ProcessGenerationResult).estimatedDuration && (
                <div><strong>Estimert varighet:</strong> {(result as ProcessGenerationResult).estimatedDuration} minutter</div>
              )}
              {onCreateProcess && (
                <button 
                  onClick={handleCreateProcess}
                  disabled={isCreating}
                  className="action-button create-button"
                >
                  {isCreating ? '📝 Oppretter...' : '📝 Opprett prosess'}
                </button>
              )}
            </div>
          )}

          {jobType === 'revision' && (
            <div className="revision-result">
              <div><strong>Sammendrag:</strong> {(result as ProcessRevisionResult).revisionSummary}</div>
              <div><strong>Endringer:</strong></div>
              <ul>
                {(result as ProcessRevisionResult).changesMade?.map((change, index) => (
                  <li key={index}>{change}</li>
                ))}
              </ul>
              {onApplyRevision && (
                <button 
                  onClick={handleApplyRevision}
                  disabled={isApplying}
                  className="action-button apply-button"
                >
                  {isApplying ? '🔄 Anvender...' : '🔄 Anvend endringer'}
                </button>
              )}
            </div>
          )}
        </div>
      )}
    </div>
  );
};