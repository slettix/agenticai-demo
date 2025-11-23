import React, { useState, useEffect } from 'react';
import { ProsessDetail, ProsessStep, StepType } from '../../types/prosess.ts';
import { prosessService } from '../../services/prosessService.ts';
import { useAuth } from '../../contexts/AuthContext.tsx';

interface ProsessDetaljerProps {
  prosessId: number;
  onBack: () => void;
}

export const ProsessDetaljer: React.FC<ProsessDetaljerProps> = ({ prosessId, onBack }) => {
  const { hasPermission } = useAuth();
  const [prosess, setProsess] = useState<ProsessDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string>('');
  const [activeTab, setActiveTab] = useState<'overview' | 'steps' | 'versions'>('overview');

  useEffect(() => {
    loadProsess();
  }, [prosessId]);

  const loadProsess = async () => {
    try {
      setLoading(true);
      const result = await prosessService.getProsess(prosessId);
      setProsess(result);
      setError('');
    } catch (err) {
      setError('Kunne ikke laste prosess');
      console.error('Error loading process:', err);
    } finally {
      setLoading(false);
    }
  };

  const getStepTypeIcon = (type: StepType): string => {
    switch (type) {
      case StepType.Start: return '🚀';
      case StepType.Action: return '⚡';
      case StepType.Decision: return '❓';
      case StepType.Approval: return '✅';
      case StepType.Review: return '👀';
      case StepType.Wait: return '⏳';
      case StepType.End: return '🏁';
      case StepType.Subprocess: return '📋';
      default: return '📄';
    }
  };

  const getStepTypeText = (type: StepType): string => {
    switch (type) {
      case StepType.Start: return 'Start';
      case StepType.Action: return 'Handling';
      case StepType.Decision: return 'Beslutning';
      case StepType.Approval: return 'Godkjenning';
      case StepType.Review: return 'Review';
      case StepType.Wait: return 'Venting';
      case StepType.End: return 'Slutt';
      case StepType.Subprocess: return 'Underprosess';
      default: return 'Ukjent';
    }
  };

  const renderStep = (step: ProsessStep, level: number = 0): React.ReactNode => {
    return (
      <div key={step.id} className={`process-step level-${level}`}>
        <div className="step-header">
          <div className="step-icon">
            {getStepTypeIcon(step.type)}
          </div>
          <div className="step-info">
            <h4 className="step-title">
              {step.title}
              {step.isOptional && <span className="optional-badge">Valgfri</span>}
            </h4>
            <div className="step-meta">
              <span className="step-type">{getStepTypeText(step.type)}</span>
              {step.responsibleRole && (
                <span className="step-role">Ansvarlig: {step.responsibleRole}</span>
              )}
              {step.estimatedDurationMinutes && (
                <span className="step-duration">
                  ⏱ {step.estimatedDurationMinutes} min
                </span>
              )}
            </div>
          </div>
        </div>
        
        <div className="step-description">
          {step.description}
        </div>
        
        {step.detailedInstructions && (
          <div className="step-instructions">
            <details>
              <summary>Detaljerte instruksjoner</summary>
              <div className="instructions-content">
                {step.detailedInstructions}
              </div>
            </details>
          </div>
        )}
        
        {step.subSteps.length > 0 && (
          <div className="sub-steps">
            <h5>Delsteg:</h5>
            {step.subSteps.map(subStep => renderStep(subStep, level + 1))}
          </div>
        )}
        
        {step.outgoingConnections.length > 0 && level === 0 && (
          <div className="step-connections">
            {step.outgoingConnections.map(conn => (
              <div key={conn.id} className="connection">
                → {conn.condition && <em>({conn.condition})</em>}
              </div>
            ))}
          </div>
        )}
      </div>
    );
  };

  if (loading) {
    return <div className="loading-spinner">Laster prosess...</div>;
  }

  if (error) {
    return (
      <div className="error-container">
        <div className="error-message">{error}</div>
        <button onClick={onBack} className="back-button">← Tilbake til liste</button>
      </div>
    );
  }

  if (!prosess) {
    return (
      <div className="error-container">
        <div className="error-message">Prosess ikke funnet</div>
        <button onClick={onBack} className="back-button">← Tilbake til liste</button>
      </div>
    );
  }

  return (
    <div className="prosess-detaljer">
      <div className="detail-header">
        <button onClick={onBack} className="back-button">← Tilbake til liste</button>
        <div className="header-content">
          <h1 className="prosess-title">{prosess.title}</h1>
          <div className="prosess-meta">
            <span className="category">{prosess.category}</span>
            <span className="view-count">👁 {prosess.viewCount} visninger</span>
            {prosess.currentVersion && (
              <span className="version">v{prosess.currentVersion.versionNumber}</span>
            )}
          </div>
          <div className="tags">
            {prosess.tags.map(tag => (
              <span 
                key={tag.id} 
                className="tag" 
                style={{ backgroundColor: tag.color }}
              >
                {tag.name}
              </span>
            ))}
          </div>
        </div>
        {hasPermission('edit_prosess') && (
          <div className="action-buttons">
            <button className="edit-button">Rediger</button>
          </div>
        )}
      </div>

      <div className="tab-navigation">
        <button 
          className={`tab ${activeTab === 'overview' ? 'active' : ''}`}
          onClick={() => setActiveTab('overview')}
        >
          📋 Oversikt
        </button>
        <button 
          className={`tab ${activeTab === 'steps' ? 'active' : ''}`}
          onClick={() => setActiveTab('steps')}
        >
          🔄 Prosessflyt ({prosess.steps.length} steg)
        </button>
        <button 
          className={`tab ${activeTab === 'versions' ? 'active' : ''}`}
          onClick={() => setActiveTab('versions')}
        >
          📚 Versjoner ({prosess.versionHistory.length})
        </button>
      </div>

      <div className="tab-content">
        {activeTab === 'overview' && (
          <div className="overview-tab">
            <div className="description-section">
              <h3>Beskrivelse</h3>
              <p>{prosess.description}</p>
            </div>
            
            {prosess.currentVersion && (
              <div className="current-version-section">
                <h3>Gjeldende versjon: {prosess.currentVersion.versionNumber}</h3>
                <div className="version-content">
                  <div dangerouslySetInnerHTML={{ 
                    __html: prosess.currentVersion.content.replace(/\n/g, '<br>') 
                  }} />
                </div>
                {prosess.currentVersion.changeLog && (
                  <div className="changelog">
                    <h4>Endringer i denne versjonen:</h4>
                    <p>{prosess.currentVersion.changeLog}</p>
                  </div>
                )}
              </div>
            )}
            
            <div className="metadata-section">
              <h3>Metadata</h3>
              <div className="metadata-grid">
                <div className="metadata-item">
                  <label>Opprettet:</label>
                  <span>{new Date(prosess.createdAt).toLocaleDateString('nb-NO')} av {prosess.createdByUserName}</span>
                </div>
                <div className="metadata-item">
                  <label>Sist oppdatert:</label>
                  <span>{new Date(prosess.updatedAt).toLocaleDateString('nb-NO')}</span>
                </div>
                {prosess.ownerName && (
                  <div className="metadata-item">
                    <label>Eier:</label>
                    <span>{prosess.ownerName}</span>
                  </div>
                )}
                {prosess.lastAccessedAt && (
                  <div className="metadata-item">
                    <label>Sist åpnet:</label>
                    <span>{new Date(prosess.lastAccessedAt).toLocaleDateString('nb-NO')}</span>
                  </div>
                )}
              </div>
            </div>
          </div>
        )}

        {activeTab === 'steps' && (
          <div className="steps-tab">
            <div className="steps-header">
              <h3>Prosessflyt</h3>
              {prosess.steps.length === 0 && (
                <p className="no-steps">Ingen steg er definert for denne prosessen ennå.</p>
              )}
            </div>
            <div className="process-flow">
              {prosess.steps
                .filter(step => !step.parentStepId) // Only show top-level steps
                .sort((a, b) => a.orderIndex - b.orderIndex)
                .map(step => renderStep(step))}
            </div>
          </div>
        )}

        {activeTab === 'versions' && (
          <div className="versions-tab">
            <div className="versions-header">
              <h3>Versjonshistorikk</h3>
            </div>
            <div className="version-list">
              {prosess.versionHistory.map(version => (
                <div 
                  key={version.id} 
                  className={`version-item ${version.isCurrent ? 'current' : ''}`}
                >
                  <div className="version-header">
                    <div className="version-number">
                      v{version.versionNumber}
                      {version.isCurrent && <span className="current-badge">Gjeldende</span>}
                      {version.isPublished && <span className="published-badge">Publisert</span>}
                    </div>
                    <div className="version-date">
                      {new Date(version.createdAt).toLocaleDateString('nb-NO')}
                    </div>
                  </div>
                  <div className="version-meta">
                    <span>Opprettet av: {version.createdByUserName}</span>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>
    </div>
  );
};