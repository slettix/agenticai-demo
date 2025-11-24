import React, { useState } from 'react';
import { ProsessStep, StepType } from '../../types/prosess.ts';
import './ProcessFlowVisualization.css';

interface ProcessFlowVisualizationProps {
  steps: ProsessStep[];
  title: string;
  description?: string;
  onStepClick?: (step: ProsessStep) => void;
  showEstimates?: boolean;
}

export const ProcessFlowVisualization: React.FC<ProcessFlowVisualizationProps> = ({
  steps,
  title,
  description,
  onStepClick,
  showEstimates = true
}) => {
  const [selectedStepId, setSelectedStepId] = useState<number | null>(null);

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

  const getStepTypeClass = (stepType: StepType): string => {
    switch (stepType) {
      case StepType.Start:
        return 'step-start';
      case StepType.Task:
        return 'step-task';
      case StepType.Document:
        return 'step-document';
      case StepType.Gateway:
        return 'step-gateway';
      case StepType.Decision:
        return 'step-decision';
      case StepType.Approval:
        return 'step-approval';
      case StepType.Review:
        return 'step-review';
      case StepType.Wait:
        return 'step-wait';
      case StepType.End:
        return 'step-end';
      case StepType.Subprocess:
        return 'step-subprocess';
      default:
        return 'step-default';
    }
  };

  const handleStepClick = (step: ProsessStep) => {
    setSelectedStepId(step.id);
    onStepClick?.(step);
  };

  const getTotalEstimatedTime = (): number => {
    return steps.reduce((total, step) => {
      return total + (step.estimatedDurationMinutes || 0);
    }, 0);
  };

  const formatDuration = (minutes: number): string => {
    if (minutes < 60) {
      return `${minutes}min`;
    }
    const hours = Math.floor(minutes / 60);
    const remainingMinutes = minutes % 60;
    return remainingMinutes > 0 ? `${hours}t ${remainingMinutes}min` : `${hours}t`;
  };

  // Sort steps by orderIndex for proper visualization
  const sortedSteps = [...steps].sort((a, b) => a.orderIndex - b.orderIndex);

  if (steps.length === 0) {
    return (
      <div className="process-flow-empty">
        <div className="empty-state">
          <div className="empty-icon">📋</div>
          <h3>Ingen prosesstrinn definert</h3>
          <p>Legg til prosesstrinn for å visualisere prosessflyt</p>
        </div>
      </div>
    );
  }

  return (
    <div className="process-flow-visualization">
      {/* Header */}
      <div className="process-flow-header">
        <div className="process-info">
          <h3>{title}</h3>
          {description && <p className="process-description">{description}</p>}
        </div>
        {showEstimates && (
          <div className="process-metrics">
            <div className="metric">
              <span className="metric-label">Antall trinn:</span>
              <span className="metric-value">{steps.length}</span>
            </div>
            <div className="metric">
              <span className="metric-label">Estimert tid:</span>
              <span className="metric-value">{formatDuration(getTotalEstimatedTime())}</span>
            </div>
          </div>
        )}
      </div>

      {/* Flow Timeline */}
      <div className="process-flow-timeline">
        <div className="timeline-container">
          {sortedSteps.map((step, index) => (
            <div key={step.id} className="timeline-item">
              {/* Step Card */}
              <div
                className={`step-card ${getStepTypeClass(step.type)} ${
                  selectedStepId === step.id ? 'selected' : ''
                } ${step.isOptional ? 'optional' : ''} ${onStepClick ? 'clickable' : ''}`}
                onClick={() => onStepClick && handleStepClick(step)}
              >
                <div className="step-number">{step.orderIndex}</div>
                <div className="step-icon">{getStepIcon(step.type)}</div>
                <div className="step-content">
                  <h4 className="step-title">{step.title}</h4>
                  <p className="step-description">{step.description}</p>
                  <div className="step-metadata">
                    {step.responsibleRole && (
                      <span className="responsible-role">
                        👤 {step.responsibleRole}
                      </span>
                    )}
                    {step.estimatedDurationMinutes && (
                      <span className="estimated-duration">
                        ⏱️ {formatDuration(step.estimatedDurationMinutes)}
                      </span>
                    )}
                    {step.isOptional && (
                      <span className="optional-badge">Valgfritt</span>
                    )}
                  </div>
                </div>
              </div>

              {/* Connector Arrow */}
              {index < sortedSteps.length - 1 && (
                <div className="step-connector">
                  <div className="connector-line"></div>
                  <div className="connector-arrow">→</div>
                </div>
              )}
            </div>
          ))}
        </div>
      </div>

      {/* Process Status Footer */}
      <div className="process-flow-footer">
        <div className="process-stats">
          <div className="stat-item">
            <span className="stat-icon">📋</span>
            <span>{steps.filter(s => !s.isOptional).length} obligatoriske trinn</span>
          </div>
          {steps.some(s => s.isOptional) && (
            <div className="stat-item">
              <span className="stat-icon">⭐</span>
              <span>{steps.filter(s => s.isOptional).length} valgfrie trinn</span>
            </div>
          )}
          <div className="stat-item">
            <span className="stat-icon">👥</span>
            <span>{new Set(steps.map(s => s.responsibleRole).filter(Boolean)).size} roller involvert</span>
          </div>
        </div>
      </div>
    </div>
  );
};