import React, { useState } from 'react';
import './ProcessStepBuilder.css';

export interface ProcessStep {
  id: string;
  title: string;
  description: string;
  type: 'Task' | 'Decision' | 'Document' | 'Approval' | 'Gateway';
  responsibleRole: string;
  estimatedDuration: number;
  orderIndex: number;
  isOptional: boolean;
  detailedInstructions: string;
  prerequisites?: string[];
  outputs?: string[];
  itilGuidance?: string;
}

interface ProcessStepBuilderProps {
  steps: ProcessStep[];
  onStepsChange: (steps: ProcessStep[]) => void;
  itilArea?: string;
  readonly?: boolean;
}

const stepTypes = [
  { value: 'Task', label: 'Oppgave', icon: '📝', description: 'En handling som må utføres' },
  { value: 'Decision', label: 'Beslutning', icon: '🤔', description: 'Et beslutningspunkt med alternativer' },
  { value: 'Document', label: 'Dokumentasjon', icon: '📄', description: 'Dokumentering eller registrering' },
  { value: 'Approval', label: 'Godkjenning', icon: '✅', description: 'Formell godkjenning eller autorisering' },
  { value: 'Gateway', label: 'Gateway', icon: '🚦', description: 'Kontrollpunkt eller validering' }
];

const commonRoles = [
  'Service Desk',
  'Process Manager', 
  'Technical Team',
  'Change Authority',
  'Incident Manager',
  'Problem Manager',
  'Team Member',
  'Business Owner',
  'IT Manager',
  'Security Team',
  'Quality Assurance'
];

export const ProcessStepBuilder: React.FC<ProcessStepBuilderProps> = ({ 
  steps, 
  onStepsChange, 
  itilArea,
  readonly = false 
}) => {
  const [editingStep, setEditingStep] = useState<string | null>(null);
  const [showAddForm, setShowAddForm] = useState(false);
  const [newStep, setNewStep] = useState<Partial<ProcessStep>>({
    title: '',
    description: '',
    type: 'Task',
    responsibleRole: '',
    estimatedDuration: 30,
    isOptional: false,
    detailedInstructions: '',
    prerequisites: [],
    outputs: []
  });

  const generateStepId = () => {
    return `step_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  };

  const addStep = () => {
    if (!newStep.title || !newStep.description || !newStep.responsibleRole) {
      return;
    }

    const step: ProcessStep = {
      id: generateStepId(),
      title: newStep.title!,
      description: newStep.description!,
      type: newStep.type as ProcessStep['type'],
      responsibleRole: newStep.responsibleRole!,
      estimatedDuration: newStep.estimatedDuration || 30,
      orderIndex: steps.length + 1,
      isOptional: newStep.isOptional || false,
      detailedInstructions: newStep.detailedInstructions || '',
      prerequisites: newStep.prerequisites || [],
      outputs: newStep.outputs || []
    };

    onStepsChange([...steps, step]);
    
    // Reset form
    setNewStep({
      title: '',
      description: '',
      type: 'Task',
      responsibleRole: '',
      estimatedDuration: 30,
      isOptional: false,
      detailedInstructions: '',
      prerequisites: [],
      outputs: []
    });
    setShowAddForm(false);
  };

  const updateStep = (stepId: string, updates: Partial<ProcessStep>) => {
    const updatedSteps = steps.map(step => 
      step.id === stepId ? { ...step, ...updates } : step
    );
    onStepsChange(updatedSteps);
  };

  const deleteStep = (stepId: string) => {
    const updatedSteps = steps
      .filter(step => step.id !== stepId)
      .map((step, index) => ({ ...step, orderIndex: index + 1 }));
    onStepsChange(updatedSteps);
  };

  const moveStep = (stepId: string, direction: 'up' | 'down') => {
    const currentIndex = steps.findIndex(step => step.id === stepId);
    if (currentIndex === -1) return;

    const newIndex = direction === 'up' ? currentIndex - 1 : currentIndex + 1;
    if (newIndex < 0 || newIndex >= steps.length) return;

    const updatedSteps = [...steps];
    [updatedSteps[currentIndex], updatedSteps[newIndex]] = 
    [updatedSteps[newIndex], updatedSteps[currentIndex]];

    // Update order indices
    updatedSteps.forEach((step, index) => {
      step.orderIndex = index + 1;
    });

    onStepsChange(updatedSteps);
  };

  const duplicateStep = (stepId: string) => {
    const stepToDuplicate = steps.find(step => step.id === stepId);
    if (!stepToDuplicate) return;

    const duplicatedStep: ProcessStep = {
      ...stepToDuplicate,
      id: generateStepId(),
      title: `${stepToDuplicate.title} (kopi)`,
      orderIndex: steps.length + 1
    };

    onStepsChange([...steps, duplicatedStep]);
  };

  const getStepTypeInfo = (type: string) => {
    return stepTypes.find(st => st.value === type) || stepTypes[0];
  };

  const getTotalDuration = () => {
    return steps.reduce((total, step) => total + step.estimatedDuration, 0);
  };

  if (readonly) {
    return (
      <div className="process-step-builder readonly">
        <div className="steps-summary">
          <h3>Prosesstrinn ({steps.length})</h3>
          <div className="duration-info">
            <span>📅 Total estimert tid: {getTotalDuration()} minutter</span>
          </div>
        </div>
        
        <div className="steps-list">
          {steps.map((step, index) => (
            <div key={step.id} className="step-item readonly">
              <div className="step-header">
                <span className="step-number">{step.orderIndex}</span>
                <span className="step-icon">{getStepTypeInfo(step.type).icon}</span>
                <h4>{step.title}</h4>
                <div className="step-meta">
                  <span className="step-type">{getStepTypeInfo(step.type).label}</span>
                  <span className="step-duration">⏱️ {step.estimatedDuration} min</span>
                  {step.isOptional && <span className="optional-badge">Valgfri</span>}
                </div>
              </div>
              <div className="step-content">
                <p className="step-description">{step.description}</p>
                <div className="step-details">
                  <div><strong>Ansvarlig:</strong> {step.responsibleRole}</div>
                  {step.detailedInstructions && (
                    <div><strong>Instruksjoner:</strong> {step.detailedInstructions}</div>
                  )}
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="process-step-builder">
      <div className="builder-header">
        <h3>Prosesstrinn ({steps.length})</h3>
        <div className="builder-controls">
          <div className="duration-info">
            <span>📅 Total estimert tid: {getTotalDuration()} minutter</span>
          </div>
          <button 
            type="button" 
            className="add-step-btn" 
            onClick={() => setShowAddForm(true)}
          >
            ➕ Legg til trinn
          </button>
        </div>
      </div>

      {itilArea && (
        <div className="itil-guidance">
          <p>💡 <strong>ITIL-område:</strong> {itilArea}. Vurder relevante ITIL-aktiviteter når du bygger prosessen.</p>
        </div>
      )}

      {showAddForm && (
        <div className="add-step-form">
          <div className="form-header">
            <h4>Nytt prosesstrinn</h4>
            <button 
              type="button" 
              className="close-btn" 
              onClick={() => setShowAddForm(false)}
            >
              ✕
            </button>
          </div>

          <div className="form-grid">
            <div className="form-group">
              <label>Trinntittel *</label>
              <input
                type="text"
                value={newStep.title || ''}
                onChange={(e) => setNewStep({ ...newStep, title: e.target.value })}
                placeholder="F.eks. Registrer hendelse"
              />
            </div>

            <div className="form-group">
              <label>Trinntype *</label>
              <select
                value={newStep.type || 'Task'}
                onChange={(e) => setNewStep({ ...newStep, type: e.target.value as ProcessStep['type'] })}
              >
                {stepTypes.map(type => (
                  <option key={type.value} value={type.value}>
                    {type.icon} {type.label}
                  </option>
                ))}
              </select>
            </div>

            <div className="form-group full-width">
              <label>Beskrivelse *</label>
              <textarea
                value={newStep.description || ''}
                onChange={(e) => setNewStep({ ...newStep, description: e.target.value })}
                placeholder="Detaljert beskrivelse av hva som skal gjøres i dette trinnet"
                rows={3}
              />
            </div>

            <div className="form-group">
              <label>Ansvarlig rolle *</label>
              <select
                value={newStep.responsibleRole || ''}
                onChange={(e) => setNewStep({ ...newStep, responsibleRole: e.target.value })}
              >
                <option value="">Velg rolle</option>
                {commonRoles.map(role => (
                  <option key={role} value={role}>{role}</option>
                ))}
              </select>
            </div>

            <div className="form-group">
              <label>Estimert tid (minutter)</label>
              <input
                type="number"
                value={newStep.estimatedDuration || 30}
                onChange={(e) => setNewStep({ ...newStep, estimatedDuration: parseInt(e.target.value) })}
                min="5"
                max="480"
              />
            </div>

            <div className="form-group full-width">
              <label>Detaljerte instruksjoner</label>
              <textarea
                value={newStep.detailedInstructions || ''}
                onChange={(e) => setNewStep({ ...newStep, detailedInstructions: e.target.value })}
                placeholder="Spesifikke instruksjoner for hvordan trinnet skal utføres"
                rows={2}
              />
            </div>

            <div className="form-group checkbox">
              <label>
                <input
                  type="checkbox"
                  checked={newStep.isOptional || false}
                  onChange={(e) => setNewStep({ ...newStep, isOptional: e.target.checked })}
                />
                Dette trinnet er valgfritt
              </label>
            </div>
          </div>

          <div className="form-actions">
            <button type="button" onClick={() => setShowAddForm(false)} className="cancel-btn">
              Avbryt
            </button>
            <button type="button" onClick={addStep} className="save-btn">
              Legg til trinn
            </button>
          </div>
        </div>
      )}

      <div className="steps-list">
        {steps.map((step, index) => (
          <div key={step.id} className={`step-item ${editingStep === step.id ? 'editing' : ''}`}>
            <div className="step-header">
              <div className="step-number">{step.orderIndex}</div>
              <div className="step-info">
                <span className="step-icon">{getStepTypeInfo(step.type).icon}</span>
                <h4>{step.title}</h4>
                <div className="step-meta">
                  <span className="step-type">{getStepTypeInfo(step.type).label}</span>
                  <span className="step-duration">⏱️ {step.estimatedDuration} min</span>
                  <span className="step-role">👤 {step.responsibleRole}</span>
                  {step.isOptional && <span className="optional-badge">Valgfri</span>}
                </div>
              </div>
              <div className="step-actions">
                <button 
                  type="button" 
                  onClick={() => moveStep(step.id, 'up')} 
                  disabled={index === 0}
                  title="Flytt opp"
                >
                  ⬆️
                </button>
                <button 
                  type="button" 
                  onClick={() => moveStep(step.id, 'down')} 
                  disabled={index === steps.length - 1}
                  title="Flytt ned"
                >
                  ⬇️
                </button>
                <button 
                  type="button" 
                  onClick={() => setEditingStep(editingStep === step.id ? null : step.id)}
                  title="Rediger"
                >
                  ✏️
                </button>
                <button 
                  type="button" 
                  onClick={() => duplicateStep(step.id)}
                  title="Dupliser"
                >
                  📋
                </button>
                <button 
                  type="button" 
                  onClick={() => deleteStep(step.id)}
                  title="Slett"
                  className="danger"
                >
                  🗑️
                </button>
              </div>
            </div>

            <div className="step-content">
              <p className="step-description">{step.description}</p>
              {step.detailedInstructions && (
                <div className="detailed-instructions">
                  <strong>Instruksjoner:</strong> {step.detailedInstructions}
                </div>
              )}
            </div>

            {editingStep === step.id && (
              <div className="step-edit-form">
                <div className="form-grid">
                  <div className="form-group">
                    <label>Trinntittel</label>
                    <input
                      type="text"
                      value={step.title}
                      onChange={(e) => updateStep(step.id, { title: e.target.value })}
                    />
                  </div>

                  <div className="form-group">
                    <label>Trinntype</label>
                    <select
                      value={step.type}
                      onChange={(e) => updateStep(step.id, { type: e.target.value as ProcessStep['type'] })}
                    >
                      {stepTypes.map(type => (
                        <option key={type.value} value={type.value}>
                          {type.icon} {type.label}
                        </option>
                      ))}
                    </select>
                  </div>

                  <div className="form-group full-width">
                    <label>Beskrivelse</label>
                    <textarea
                      value={step.description}
                      onChange={(e) => updateStep(step.id, { description: e.target.value })}
                      rows={3}
                    />
                  </div>

                  <div className="form-group">
                    <label>Ansvarlig rolle</label>
                    <select
                      value={step.responsibleRole}
                      onChange={(e) => updateStep(step.id, { responsibleRole: e.target.value })}
                    >
                      {commonRoles.map(role => (
                        <option key={role} value={role}>{role}</option>
                      ))}
                    </select>
                  </div>

                  <div className="form-group">
                    <label>Estimert tid (minutter)</label>
                    <input
                      type="number"
                      value={step.estimatedDuration}
                      onChange={(e) => updateStep(step.id, { estimatedDuration: parseInt(e.target.value) })}
                      min="5"
                      max="480"
                    />
                  </div>

                  <div className="form-group full-width">
                    <label>Detaljerte instruksjoner</label>
                    <textarea
                      value={step.detailedInstructions}
                      onChange={(e) => updateStep(step.id, { detailedInstructions: e.target.value })}
                      rows={2}
                    />
                  </div>

                  <div className="form-group checkbox">
                    <label>
                      <input
                        type="checkbox"
                        checked={step.isOptional}
                        onChange={(e) => updateStep(step.id, { isOptional: e.target.checked })}
                      />
                      Dette trinnet er valgfritt
                    </label>
                  </div>
                </div>
                
                <div className="edit-actions">
                  <button 
                    type="button" 
                    onClick={() => setEditingStep(null)}
                    className="done-btn"
                  >
                    Ferdig
                  </button>
                </div>
              </div>
            )}
          </div>
        ))}

        {steps.length === 0 && (
          <div className="empty-steps">
            <p>Ingen trinn lagt til ennå. Klikk "Legg til trinn" for å begynne å bygge prosessen.</p>
          </div>
        )}
      </div>
    </div>
  );
};