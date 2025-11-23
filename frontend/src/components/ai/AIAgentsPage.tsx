import React, { useState, useEffect } from 'react';
import { ProcessGeneratorForm } from './ProcessGeneratorForm.tsx';
import { ProcessRevisionForm } from './ProcessRevisionForm.tsx';
import { JobStatusTracker } from './JobStatusTracker.tsx';
import { agentService } from '../../services/agentService.ts';
import { useAuth } from '../../contexts/AuthContext.tsx';
import type { AgentServiceHealth, ProcessGenerationResult, ProcessRevisionResult } from '../../types/agent.ts';

interface ActiveJob {
  id: string;
  type: 'generation' | 'revision';
  processId?: number;
  processTitle?: string;
}

export const AIAgentsPage: React.FC = () => {
  const { hasRole } = useAuth();
  const [activeView, setActiveView] = useState<'generate' | 'revise' | 'status'>('generate');
  const [serviceHealth, setServiceHealth] = useState<AgentServiceHealth | null>(null);
  const [healthError, setHealthError] = useState<string>('');
  const [activeJobs, setActiveJobs] = useState<ActiveJob[]>([]);
  const [error, setError] = useState<string>('');
  const [success, setSuccess] = useState<string>('');

  // Mock process for revision demo
  const [selectedProcess] = useState({
    id: 1,
    title: 'Employee Onboarding Process'
  });

  useEffect(() => {
    checkServiceHealth();
    // Check health every 30 seconds
    const interval = setInterval(checkServiceHealth, 30000);
    return () => clearInterval(interval);
  }, []);

  const checkServiceHealth = async () => {
    try {
      const health = await agentService.getHealth();
      setServiceHealth(health);
      setHealthError('');
    } catch (err) {
      setHealthError(err instanceof Error ? err.message : 'Failed to check service health');
      setServiceHealth(null);
    }
  };

  const handleJobSubmitted = (jobId: string, type: 'generation' | 'revision', processId?: number, processTitle?: string) => {
    const newJob: ActiveJob = {
      id: jobId,
      type,
      processId,
      processTitle
    };
    
    setActiveJobs(prev => [...prev, newJob]);
    setActiveView('status');
    setError('');
    setSuccess(`${type === 'generation' ? 'Genereringsjobbe' : 'Revisjonsjobb'} startet! Jobb ID: ${jobId}`);
  };

  const handleJobCompleted = (result: ProcessGenerationResult | ProcessRevisionResult) => {
    setSuccess('Jobb fullført! Se resultatet nedenfor.');
    setError('');
  };

  const handleJobFailed = (error: string) => {
    setError(`Jobb feilet: ${error}`);
    setSuccess('');
  };

  const handleCreateProcess = async (jobId: string) => {
    try {
      await agentService.createProcessFromGeneration(jobId);
      setSuccess('Prosess opprettet fra AI-generering!');
      setError('');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create process');
    }
  };

  const handleApplyRevision = async (jobId: string) => {
    try {
      await agentService.applyRevision(jobId);
      setSuccess('AI-revisjon anvendt på prosessen!');
      setError('');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to apply revision');
    }
  };

  const clearMessages = () => {
    setError('');
    setSuccess('');
  };

  const canUseAI = hasRole('Admin') || hasRole('ProsessEier') || hasRole('QA');

  if (!canUseAI) {
    return (
      <div className="ai-page">
        <div className="access-denied">
          <h2>🚫 Tilgang nektet</h2>
          <p>Du har ikke tilgang til AI-agentene. Kontakt administrator for tilgang.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="ai-page">
      <header className="ai-header">
        <h1>🤖 AI Agenter</h1>
        <p>Bruk kunstig intelligens til å generere og forbedre forretningsprosesser</p>
        
        {/* Service Health Status */}
        <div className={`health-status ${serviceHealth?.status || 'unknown'}`}>
          {serviceHealth ? (
            <>
              <span className="health-icon">
                {serviceHealth.status === 'healthy' ? '🟢' : 
                 serviceHealth.status === 'degraded' ? '🟡' : '🔴'}
              </span>
              <span className="health-text">{serviceHealth.message}</span>
            </>
          ) : (
            <>
              <span className="health-icon">🔴</span>
              <span className="health-text">
                {healthError || 'AI-agenter er ikke tilgjengelige'}
              </span>
            </>
          )}
        </div>
      </header>

      {/* Navigation */}
      <nav className="ai-nav">
        <button 
          className={activeView === 'generate' ? 'active' : ''}
          onClick={() => setActiveView('generate')}
          disabled={!serviceHealth?.agentServiceAvailable}
        >
          🚀 Generer prosess
        </button>
        <button 
          className={activeView === 'revise' ? 'active' : ''}
          onClick={() => setActiveView('revise')}
          disabled={!serviceHealth?.agentServiceAvailable}
        >
          🔧 Revider prosess
        </button>
        <button 
          className={activeView === 'status' ? 'active' : ''}
          onClick={() => setActiveView('status')}
        >
          📊 Jobbstatus
          {activeJobs.length > 0 && <span className="job-count">{activeJobs.length}</span>}
        </button>
      </nav>

      {/* Messages */}
      {(error || success) && (
        <div className="message-container">
          {error && (
            <div className="message error-message">
              <span>❌ {error}</span>
              <button onClick={clearMessages} className="close-button">×</button>
            </div>
          )}
          {success && (
            <div className="message success-message">
              <span>✅ {success}</span>
              <button onClick={clearMessages} className="close-button">×</button>
            </div>
          )}
        </div>
      )}

      {/* Main Content */}
      <main className="ai-content">
        {!serviceHealth?.agentServiceAvailable && (
          <div className="service-unavailable">
            <h3>🔧 AI-tjenester er ikke tilgjengelige</h3>
            <p>AI-agentene er for øyeblikket ikke tilgjengelige. Dette kan skyldes:</p>
            <ul>
              <li>Agente-tjenesten er ikke startet</li>
              <li>Nettverksproblemer</li>
              <li>Konfigurasjonsut</li>
            </ul>
            <button onClick={checkServiceHealth} className="retry-button">
              🔄 Prøv igjen
            </button>
          </div>
        )}

        {serviceHealth?.agentServiceAvailable && (
          <>
            {activeView === 'generate' && (
              <ProcessGeneratorForm
                onJobSubmitted={(jobId) => handleJobSubmitted(jobId, 'generation')}
                onError={setError}
              />
            )}

            {activeView === 'revise' && (
              <ProcessRevisionForm
                processId={selectedProcess.id}
                processTitle={selectedProcess.title}
                onJobSubmitted={(jobId) => handleJobSubmitted(jobId, 'revision', selectedProcess.id, selectedProcess.title)}
                onError={setError}
              />
            )}

            {activeView === 'status' && (
              <div className="job-status-container">
                <h2>📊 Aktive AI-jobber</h2>
                
                {activeJobs.length === 0 ? (
                  <div className="no-jobs">
                    <p>Ingen aktive jobber for øyeblikket.</p>
                    <p>Start en ny jobb ved å generere eller revidere en prosess.</p>
                  </div>
                ) : (
                  <div className="jobs-list">
                    {activeJobs.map((job) => (
                      <JobStatusTracker
                        key={job.id}
                        jobId={job.id}
                        jobType={job.type}
                        onJobCompleted={handleJobCompleted}
                        onJobFailed={handleJobFailed}
                        onCreateProcess={job.type === 'generation' ? handleCreateProcess : undefined}
                        onApplyRevision={job.type === 'revision' ? handleApplyRevision : undefined}
                      />
                    ))}
                  </div>
                )}
              </div>
            )}
          </>
        )}
      </main>
    </div>
  );
};