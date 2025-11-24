import React from 'react';
import { ProsessStep, StepType } from '../../types/prosess.ts';
import './ProcessStepDetailModal.css';

interface ProcessStepDetailModalProps {
  step: ProsessStep;
  stepNumber: number;
  totalSteps: number;
  onClose: () => void;
  onPrevious?: () => void;
  onNext?: () => void;
  showNavigation?: boolean;
}

export const ProcessStepDetailModal: React.FC<ProcessStepDetailModalProps> = ({
  step,
  stepNumber,
  totalSteps,
  onClose,
  onPrevious,
  onNext,
  showNavigation = true
}) => {
  const getStepIcon = (stepType: StepType): string => {
    switch (stepType) {
      case StepType.Start:
        return '🚀';
      case StepType.Task:
        return '⚡';
      case StepType.Document:
        return '📄';
      case StepType.Gateway:
        return '🔀';
      case StepType.Decision:
        return '❓';
      case StepType.Approval:
        return '✅';
      case StepType.Review:
        return '👀';
      case StepType.Wait:
        return '⏱️';
      case StepType.End:
        return '🏁';
      case StepType.Subprocess:
        return '🔄';
      default:
        return '📋';
    }
  };

  const getStepTypeName = (stepType: StepType): string => {
    switch (stepType) {
      case StepType.Start:
        return 'Start';
      case StepType.Task:
        return 'Oppgave';
      case StepType.Document:
        return 'Dokumentasjon';
      case StepType.Gateway:
        return 'Gateway';
      case StepType.Decision:
        return 'Beslutning';
      case StepType.Approval:
        return 'Godkjenning';
      case StepType.Review:
        return 'Gjennomgang';
      case StepType.Wait:
        return 'Venting';
      case StepType.End:
        return 'Slutt';
      case StepType.Subprocess:
        return 'Underprosess';
      default:
        return 'Ukjent';
    }
  };

  const formatDuration = (minutes: number): string => {
    if (minutes < 60) {
      return `${minutes} minutter`;
    }
    const hours = Math.floor(minutes / 60);
    const remainingMinutes = minutes % 60;
    return remainingMinutes > 0 
      ? `${hours} timer og ${remainingMinutes} minutter`
      : `${hours} timer`;
  };

  const getITILGuidance = (): string[] => {
    // Mock ITIL guidance based on step type
    switch (step.type) {
      case StepType.Task:
        return [
          'Sørg for at alle nødvendige ressurser er tilgjengelige',
          'Dokumenter alle utførte handlinger',
          'Følg etablerte prosedyrer og retningslinjer'
        ];
      case StepType.Decision:
        return [
          'Bruk definerte kriterier for beslutning',
          'Involver riktige interessenter',
          'Dokumenter begrunnelse for beslutning'
        ];
      case StepType.Approval:
        return [
          'Sørg for at alle krav er oppfylt før godkjenning',
          'Verifiser at riktig person gir godkjenning',
          'Dokumenter godkjenning med tidsstempel'
        ];
      case StepType.Review:
        return [
          'Sjekk kvalitet og fullstendighet',
          'Sammenlign med definerte standarder',
          'Gi konstruktive tilbakemeldinger'
        ];
      default:
        return [
          'Følg etablerte ITIL-retningslinjer',
          'Sørg for kontinuerlig forbedring',
          'Dokumenter alle aktiviteter'
        ];
    }
  };

  return (
    <div className="process-step-detail-overlay" onClick={onClose}>
      <div className="process-step-detail-modal" onClick={(e) => e.stopPropagation()}>
        {/* Header */}
        <div className="modal-header">
          <div className="step-progress">
            <span className="step-counter">Steg {stepNumber} av {totalSteps}</span>
            <div className="progress-bar">
              <div 
                className="progress-fill" 
                style={{ width: `${(stepNumber / totalSteps) * 100}%` }}
              ></div>
            </div>
          </div>
          <button className="close-button" onClick={onClose}>
            ✕
          </button>
        </div>

        {/* Step Content */}
        <div className="modal-content">
          {/* Step Header */}
          <div className="step-header">
            <div className="step-icon-large">{getStepIcon(step.type)}</div>
            <div className="step-info">
              <h2 className="step-title">{step.title}</h2>
              <div className="step-type-badge">
                {getStepTypeName(step.type)}
                {step.isOptional && <span className="optional-indicator">• Valgfritt</span>}
              </div>
            </div>
          </div>

          {/* Step Description */}
          <div className="step-section">
            <h3>📝 Beskrivelse</h3>
            <p className="step-description">{step.description}</p>
          </div>

          {/* Detailed Instructions */}
          {step.detailedInstructions && (
            <div className="step-section">
              <h3>📋 Detaljerte instruksjoner</h3>
              <div className="detailed-instructions">
                {step.detailedInstructions.split('\n').map((line, index) => (
                  <p key={index}>{line}</p>
                ))}
              </div>
            </div>
          )}

          {/* ITIL Guidance */}
          <div className="step-section">
            <h3>🎯 ITIL-veiledning</h3>
            <ul className="itil-guidance">
              {getITILGuidance().map((guidance, index) => (
                <li key={index}>{guidance}</li>
              ))}
            </ul>
          </div>

          {/* Step Metadata */}
          <div className="step-metadata-section">
            <h3>ℹ️ Detaljer</h3>
            <div className="metadata-grid">
              {step.responsibleRole && (
                <div className="metadata-item">
                  <span className="metadata-label">👤 Ansvarlig rolle:</span>
                  <span className="metadata-value">{step.responsibleRole}</span>
                </div>
              )}
              {step.estimatedDurationMinutes && (
                <div className="metadata-item">
                  <span className="metadata-label">⏱️ Estimert tid:</span>
                  <span className="metadata-value">{formatDuration(step.estimatedDurationMinutes)}</span>
                </div>
              )}
              <div className="metadata-item">
                <span className="metadata-label">📋 Type:</span>
                <span className="metadata-value">{getStepTypeName(step.type)}</span>
              </div>
              <div className="metadata-item">
                <span className="metadata-label">🏷️ Status:</span>
                <span className="metadata-value">
                  {step.isOptional ? 'Valgfritt' : 'Obligatorisk'}
                </span>
              </div>
            </div>
          </div>

          {/* Connections */}
          {step.outgoingConnections && step.outgoingConnections.length > 0 && (
            <div className="step-section">
              <h3>🔗 Tilkoblinger</h3>
              <div className="connections-list">
                {step.outgoingConnections.map((connection, index) => (
                  <div key={connection.id} className="connection-item">
                    <span className="connection-type">→</span>
                    <span className="connection-details">
                      Til steg {connection.toStepId}
                      {connection.condition && (
                        <span className="connection-condition">
                          (Betingelse: {connection.condition})
                        </span>
                      )}
                    </span>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>

        {/* Footer with Navigation */}
        {showNavigation && (
          <div className="modal-footer">
            <button 
              className="nav-button prev-button" 
              onClick={onPrevious}
              disabled={stepNumber <= 1}
            >
              ← Forrige steg
            </button>
            <span className="step-position">Steg {stepNumber} av {totalSteps}</span>
            <button 
              className="nav-button next-button" 
              onClick={onNext}
              disabled={stepNumber >= totalSteps}
            >
              Neste steg →
            </button>
          </div>
        )}
      </div>
    </div>
  );
};