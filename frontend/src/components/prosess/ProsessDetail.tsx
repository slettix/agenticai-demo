import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { prosessService } from '../../services/prosessService.ts';
import { ProsessDetail as ProsessDetailType, ProsessStep } from '../../types/prosess.ts';
import { ProcessFlowVisualization } from './ProcessFlowVisualization.tsx';
import { ProcessStepDetailModal } from './ProcessStepDetailModal.tsx';
import './ProsessDetail.css';

export const ProsessDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [prosess, setProsess] = useState<ProsessDetailType | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedStep, setSelectedStep] = useState<ProsessStep | null>(null);
  const [selectedStepIndex, setSelectedStepIndex] = useState<number>(-1);

  useEffect(() => {
    const loadProsess = async () => {
      if (!id) {
        setError('Prosess ID mangler');
        setLoading(false);
        return;
      }

      try {
        const prosessDetail = await prosessService.getProsess(parseInt(id));
        if (prosessDetail) {
          setProsess(prosessDetail);
          // Record view
          await prosessService.recordView(parseInt(id));
        } else {
          setError('Prosessen ble ikke funnet');
        }
      } catch (err) {
        console.error('Error loading prosess:', err);
        setError('Kunne ikke laste prosessen');
      } finally {
        setLoading(false);
      }
    };

    loadProsess();
  }, [id]);

  const handleStepClick = (step: ProsessStep) => {
    setSelectedStep(step);
    const stepIndex = prosess?.steps.findIndex(s => s.id === step.id) ?? -1;
    setSelectedStepIndex(stepIndex);
  };

  const handleCloseModal = () => {
    setSelectedStep(null);
    setSelectedStepIndex(-1);
  };

  const handlePreviousStep = () => {
    if (!prosess || selectedStepIndex <= 0) return;
    const prevStep = prosess.steps[selectedStepIndex - 1];
    setSelectedStep(prevStep);
    setSelectedStepIndex(selectedStepIndex - 1);
  };

  const handleNextStep = () => {
    if (!prosess || selectedStepIndex >= prosess.steps.length - 1) return;
    const nextStep = prosess.steps[selectedStepIndex + 1];
    setSelectedStep(nextStep);
    setSelectedStepIndex(selectedStepIndex + 1);
  };

  const getStatusColor = (status: number): string => {
    switch (status) {
      case 0: return '#6c757d'; // Draft
      case 1: return '#ffc107'; // InReview
      case 2: return '#28a745'; // Approved
      case 3: return '#007bff'; // Published
      case 4: return '#fd7e14'; // Deprecated
      case 5: return '#dc3545'; // Archived
      default: return '#6c757d';
    }
  };

  const getStatusText = (status: number): string => {
    switch (status) {
      case 0: return 'Utkast';
      case 1: return 'Til gjennomgang';
      case 2: return 'Godkjent';
      case 3: return 'Publisert';
      case 4: return 'Utdatert';
      case 5: return 'Arkivert';
      default: return 'Ukjent';
    }
  };

  const formatDate = (dateString: string): string => {
    return new Date(dateString).toLocaleDateString('nb-NO', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const getTotalDuration = (): number => {
    if (!prosess?.steps) return 0;
    return prosess.steps.reduce((total, step) => total + (step.estimatedDurationMinutes || 0), 0);
  };

  if (loading) {
    return (
      <div className="prosess-detail-loading">
        <div className="spinner"></div>
        <p>Laster prosess...</p>
      </div>
    );
  }

  if (error || !prosess) {
    return (
      <div className="prosess-detail-error">
        <div className="error-content">
          <h2>⚠️ Feil</h2>
          <p>{error || 'Prosessen ble ikke funnet'}</p>
          <button 
            className="btn-secondary" 
            onClick={() => navigate('/prosesser')}
          >
            Tilbake til oversikt
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="prosess-detail">
      {/* Header */}
      <div className="prosess-header">
        <div className="header-content">
          <div className="breadcrumb">
            <button className="breadcrumb-link" onClick={() => navigate('/prosesser')}>
              Prosessoversikt
            </button>
            <span className="breadcrumb-separator">→</span>
            <span className="current-page">{prosess.title}</span>
          </div>
          
          <div className="prosess-title-section">
            <h1>{prosess.title}</h1>
            <div className="prosess-meta">
              <span 
                className="status-badge" 
                style={{ backgroundColor: getStatusColor(prosess.status) }}
              >
                {getStatusText(prosess.status)}
              </span>
              <span className="category-badge">{prosess.category}</span>
              <span className="view-count">👁️ {prosess.viewCount} visninger</span>
            </div>
          </div>

          <div className="prosess-actions">
            <button className="btn-secondary">
              📊 Statistikk
            </button>
            <button className="btn-secondary">
              📝 Rediger
            </button>
            <button className="btn-primary">
              ▶️ Start prosess
            </button>
          </div>
        </div>
      </div>

      {/* Content */}
      <div className="prosess-content">
        {/* Overview */}
        <div className="prosess-overview">
          <div className="overview-card">
            <h3>📋 Beskrivelse</h3>
            <p>{prosess.description}</p>
          </div>

          <div className="overview-stats">
            <div className="stat-card">
              <div className="stat-icon">⚡</div>
              <div className="stat-content">
                <span className="stat-value">{prosess.steps.length}</span>
                <span className="stat-label">Prosesstrinn</span>
              </div>
            </div>
            
            <div className="stat-card">
              <div className="stat-icon">⏱️</div>
              <div className="stat-content">
                <span className="stat-value">
                  {Math.round(getTotalDuration() / 60) || '<1'}t
                </span>
                <span className="stat-label">Estimert tid</span>
              </div>
            </div>

            <div className="stat-card">
              <div className="stat-icon">👥</div>
              <div className="stat-content">
                <span className="stat-value">
                  {new Set(prosess.steps.map(s => s.responsibleRole).filter(Boolean)).size}
                </span>
                <span className="stat-label">Roller involvert</span>
              </div>
            </div>

            <div className="stat-card">
              <div className="stat-icon">📅</div>
              <div className="stat-content">
                <span className="stat-value">
                  {formatDate(prosess.updatedAt).split(' ')[0]}
                </span>
                <span className="stat-label">Sist oppdatert</span>
              </div>
            </div>
          </div>
        </div>

        {/* Process Flow Visualization */}
        {prosess.steps && prosess.steps.length > 0 ? (
          <ProcessFlowVisualization
            steps={prosess.steps}
            title="Prosessflyt"
            description="Klikk på et steg for å se detaljer og veiledning"
            onStepClick={handleStepClick}
            showEstimates={true}
          />
        ) : (
          <div className="no-steps-message">
            <div className="empty-state">
              <div className="empty-icon">📋</div>
              <h3>Ingen prosesstrinn definert</h3>
              <p>Denne prosessen har ikke definert noen trinn ennå.</p>
              <button className="btn-primary">
                ➕ Legg til trinn
              </button>
            </div>
          </div>
        )}

        {/* Additional Info */}
        <div className="prosess-additional-info">
          <div className="info-section">
            <h3>🏷️ Tags</h3>
            {prosess.tags && prosess.tags.length > 0 ? (
              <div className="tags-container">
                {prosess.tags.map(tag => (
                  <span 
                    key={tag.id} 
                    className="tag" 
                    style={{ backgroundColor: tag.color + '20', color: tag.color }}
                  >
                    {tag.name}
                  </span>
                ))}
              </div>
            ) : (
              <p className="no-tags">Ingen tags definert</p>
            )}
          </div>

          <div className="info-section">
            <h3>ℹ️ Prosessdetaljer</h3>
            <div className="details-grid">
              <div className="detail-item">
                <span className="detail-label">Opprettet av:</span>
                <span className="detail-value">{prosess.createdByUserName}</span>
              </div>
              <div className="detail-item">
                <span className="detail-label">Opprettet:</span>
                <span className="detail-value">{formatDate(prosess.createdAt)}</span>
              </div>
              {prosess.ownerName && (
                <div className="detail-item">
                  <span className="detail-label">Eier:</span>
                  <span className="detail-value">{prosess.ownerName}</span>
                </div>
              )}
              <div className="detail-item">
                <span className="detail-label">Sist oppdatert:</span>
                <span className="detail-value">{formatDate(prosess.updatedAt)}</span>
              </div>
              {prosess.lastAccessedAt && (
                <div className="detail-item">
                  <span className="detail-label">Sist åpnet:</span>
                  <span className="detail-value">{formatDate(prosess.lastAccessedAt)}</span>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Step Detail Modal */}
      {selectedStep && (
        <ProcessStepDetailModal
          step={selectedStep}
          stepNumber={selectedStepIndex + 1}
          totalSteps={prosess.steps.length}
          onClose={handleCloseModal}
          onPrevious={handlePreviousStep}
          onNext={handleNextStep}
          showNavigation={true}
        />
      )}
    </div>
  );
};