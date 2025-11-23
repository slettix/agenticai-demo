import React, { useState, useEffect } from 'react';
import { useAuth } from '../../contexts/AuthContext';
import { prosessService } from '../../services/prosessService';
import { ProcessStepBuilder, ProcessStep } from './ProcessStepBuilder';
import { AIProcessSuggestions } from './AIProcessSuggestions';
import './CreateProsessForm.css';

interface CreateProsessFormProps {
  onSuccess: (prosessId: number) => void;
  onCancel: () => void;
}

interface ITILArea {
  name: string;
  description: string;
  commonProcesses: string[];
}

interface ITILTemplate {
  name: string;
  itilArea: string;
  purpose: string;
  description: string;
  keyActivities: string[];
  inputs: string[];
  outputs: string[];
  kpis: string[];
  defaultSteps: any[];
}

interface ProsessCategories {
  businessCategories: string[];
  itilAreas: ITILArea[];
  priorities: string[];
}

export const CreateProsessForm: React.FC<CreateProsessFormProps> = ({ onSuccess, onCancel }) => {
  const { user } = useAuth();
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    category: '',
    itilArea: '',
    priority: 'Medium',
    tags: ''
  });
  
  const [processSteps, setProcessSteps] = useState<ProcessStep[]>([]);
  const [stepBuilderMode, setStepBuilderMode] = useState<'manual' | 'ai' | 'template'>('manual');
  const [showAISuggestions, setShowAISuggestions] = useState(false);
  const [generatingAISteps, setGeneratingAISteps] = useState(false);
  
  const [categories, setCategories] = useState<ProsessCategories | null>(null);
  const [templates, setTemplates] = useState<ITILTemplate[]>([]);
  const [selectedTemplate, setSelectedTemplate] = useState<ITILTemplate | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showTemplatePreview, setShowTemplatePreview] = useState(false);

  useEffect(() => {
    loadCategories();
  }, []);

  useEffect(() => {
    if (formData.itilArea) {
      loadTemplatesForArea(formData.itilArea);
    } else {
      setTemplates([]);
      setSelectedTemplate(null);
    }
  }, [formData.itilArea]);

  const loadCategories = async () => {
    try {
      const categoriesData = await prosessService.getCategories();
      setCategories(categoriesData);
    } catch (err) {
      setError('Kunne ikke laste kategorier');
    }
  };

  const loadTemplatesForArea = async (area: string) => {
    try {
      const templatesData = await prosessService.getITILTemplates(area);
      setTemplates(templatesData);
    } catch (err) {
      console.error('Kunne ikke laste ITIL-maler:', err);
      setTemplates([]);
    }
  };

  const handleInputChange = (field: string, value: string) => {
    setFormData(prev => ({
      ...prev,
      [field]: value
    }));
    setError(null);
  };

  const handleTemplateSelect = (template: ITILTemplate) => {
    setSelectedTemplate(template);
    setFormData(prev => ({
      ...prev,
      title: template.name,
      description: template.description,
      category: 'ITSM'
    }));
    setShowTemplatePreview(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError(null);

    try {
      if (!formData.title.trim() || !formData.description.trim() || !formData.category) {
        throw new Error('Tittel, beskrivelse og kategori er påkrevd');
      }

      const requestData = {
        title: formData.title.trim(),
        description: formData.description.trim(),
        category: formData.category,
        itilArea: formData.itilArea || undefined,
        priority: formData.priority,
        tags: formData.tags ? formData.tags.split(',').map(t => t.trim()).filter(t => t) : undefined,
        processSteps: stepBuilderMode === 'manual' ? processSteps : undefined,
        stepBuilderMode
      };

      const newProsess = await prosessService.createProsess(requestData);
      
      if (newProsess) {
        onSuccess(newProsess.id);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'En feil oppstod under opprettelse av prosess');
    } finally {
      setIsLoading(false);
    }
  };

  const handleCancel = () => {
    if (hasUnsavedChanges()) {
      if (window.confirm('Du har ulagrede endringer. Er du sikker på at du vil avbryte?')) {
        onCancel();
      }
    } else {
      onCancel();
    }
  };

  const hasUnsavedChanges = () => {
    return formData.title || formData.description || formData.category || formData.itilArea || formData.tags || processSteps.length > 0;
  };

  const handleApplyAISuggestion = (suggestion: any) => {
    if (suggestion.type === 'step' && suggestion.suggestedStep) {
      // Add the suggested step to the current process steps
      setProcessSteps(prevSteps => [...prevSteps, suggestion.suggestedStep]);
      setStepBuilderMode('manual'); // Switch to manual mode to show the added step
    } else if (suggestion.type === 'improvement') {
      // Handle improvement suggestions (could show a modal or apply changes)
      alert(`Forbedring anvendt: ${suggestion.title}\n\n${suggestion.description}`);
    } else if (suggestion.type === 'template') {
      // Switch to template mode if a template is suggested
      setStepBuilderMode('template');
    }
  };

  const handleGenerateAISteps = async () => {
    setGeneratingAISteps(true);
    setStepBuilderMode('ai');
    
    try {
      // Simulate AI generation process
      await new Promise(resolve => setTimeout(resolve, 2000));
      
      // In a real implementation, this would call the backend to generate steps
      // For now, we'll just show the AI mode
      alert('AI-generering av prosesstrinn vil bli implementert i backend-integrasjonen.');
      
    } catch (error) {
      setError('Kunne ikke generere AI-trinn. Prøv igjen senere.');
    } finally {
      setGeneratingAISteps(false);
    }
  };

  if (!categories) {
    return <div className="loading">Laster kategorier...</div>;
  }

  return (
    <div className="create-prosess-form">
      <div className="form-header">
        <h2>📝 Opprett ny prosess</h2>
        <p>Opprett en ny prosessbeskrivelse manuelt eller ved å bruke ITIL-maler</p>
      </div>

      <form onSubmit={handleSubmit} className="prosess-form">
        {/* Grunnleggende informasjon */}
        <div className="form-section">
          <h3>Grunnleggende informasjon</h3>
          
          <div className="form-group">
            <label htmlFor="title">Prosesstittel *</label>
            <input
              type="text"
              id="title"
              value={formData.title}
              onChange={(e) => handleInputChange('title', e.target.value)}
              placeholder="F.eks. Incident Management, Onboarding av nye medarbeidere"
              maxLength={200}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="description">Beskrivelse *</label>
            <textarea
              id="description"
              value={formData.description}
              onChange={(e) => handleInputChange('description', e.target.value)}
              placeholder="Beskriv prosessens formål og anvendelse..."
              rows={4}
              maxLength={1000}
              required
            />
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="category">Kategori *</label>
              <select
                id="category"
                value={formData.category}
                onChange={(e) => handleInputChange('category', e.target.value)}
                required
              >
                <option value="">Velg kategori</option>
                <optgroup label="Forretningsområder">
                  {categories.businessCategories.map(cat => (
                    <option key={cat} value={cat}>{cat}</option>
                  ))}
                </optgroup>
                <optgroup label="ITSM">
                  <option value="ITSM">IT Service Management</option>
                </optgroup>
              </select>
            </div>

            <div className="form-group">
              <label htmlFor="priority">Prioritet</label>
              <select
                id="priority"
                value={formData.priority}
                onChange={(e) => handleInputChange('priority', e.target.value)}
              >
                {categories.priorities.map(priority => (
                  <option key={priority} value={priority}>{priority}</option>
                ))}
              </select>
            </div>
          </div>
        </div>

        {/* ITIL-integrasjon */}
        <div className="form-section">
          <h3>ITIL-integrasjon (valgfritt)</h3>
          
          <div className="form-group">
            <label htmlFor="itilArea">ITIL-område</label>
            <select
              id="itilArea"
              value={formData.itilArea}
              onChange={(e) => handleInputChange('itilArea', e.target.value)}
            >
              <option value="">Velg ITIL-område (valgfritt)</option>
              {categories.itilAreas.map(area => (
                <option key={area.name} value={area.name}>{area.name}</option>
              ))}
            </select>
            {formData.itilArea && (
              <small className="field-help">
                {categories.itilAreas.find(a => a.name === formData.itilArea)?.description}
              </small>
            )}
          </div>

          {/* ITIL-maler */}
          {templates.length > 0 && (
            <div className="form-group">
              <label>ITIL-prosessmaler</label>
              <div className="template-grid">
                {templates.map(template => (
                  <div
                    key={template.name}
                    className={`template-card ${selectedTemplate?.name === template.name ? 'selected' : ''}`}
                    onClick={() => handleTemplateSelect(template)}
                  >
                    <h4>{template.name}</h4>
                    <p>{template.purpose}</p>
                    <div className="template-meta">
                      <span>📋 {template.defaultSteps.length} steg</span>
                      <span>📊 {template.kpis.length} KPI-er</span>
                    </div>
                  </div>
                ))}
              </div>
              {selectedTemplate && showTemplatePreview && (
                <div className="template-preview">
                  <h4>📋 Forhåndsvisning: {selectedTemplate.name}</h4>
                  <p><strong>Formål:</strong> {selectedTemplate.purpose}</p>
                  <p><strong>Nøkkelaktiviteter:</strong> {selectedTemplate.keyActivities.join(', ')}</p>
                  <p><strong>KPI-er:</strong> {selectedTemplate.kpis.join(', ')}</p>
                  <button
                    type="button"
                    className="btn-secondary small"
                    onClick={() => setShowTemplatePreview(false)}
                  >
                    Skjul forhåndsvisning
                  </button>
                </div>
              )}
            </div>
          )}
        </div>

        {/* Metadata */}
        <div className="form-section">
          <h3>Metadata</h3>
          
          <div className="form-group">
            <label htmlFor="tags">Tags (kommaseparert)</label>
            <input
              type="text"
              id="tags"
              value={formData.tags}
              onChange={(e) => handleInputChange('tags', e.target.value)}
              placeholder="F.eks. kritisk, automatisert, kundevendt"
            />
            <small className="field-help">
              Skill tags med komma. Eksempler: kritisk, automatisert, manuell, kundevendt
            </small>
          </div>
        </div>

        {/* Process Steps Builder */}
        <div className="form-section">
          <h3>Prosesstrinn</h3>
          
          <div className="step-builder-options">
            <div className="builder-mode-selector">
              <label>Velg metode for å bygge prosesstrinn:</label>
              <div className="mode-buttons">
                <button
                  type="button"
                  className={`mode-btn ${stepBuilderMode === 'manual' ? 'active' : ''}`}
                  onClick={() => setStepBuilderMode('manual')}
                >
                  🔧 Manuell bygging
                </button>
                <button
                  type="button"
                  className={`mode-btn ${stepBuilderMode === 'ai' ? 'active' : ''}`}
                  onClick={() => setStepBuilderMode('ai')}
                >
                  🤖 AI-generert
                </button>
                {selectedTemplate && (
                  <button
                    type="button"
                    className={`mode-btn ${stepBuilderMode === 'template' ? 'active' : ''}`}
                    onClick={() => setStepBuilderMode('template')}
                  >
                    📋 Fra mal ({selectedTemplate.name})
                  </button>
                )}
              </div>
            </div>
          </div>

          {stepBuilderMode === 'manual' && (
            <ProcessStepBuilder
              steps={processSteps}
              onStepsChange={setProcessSteps}
              itilArea={formData.itilArea}
            />
          )}

          {stepBuilderMode === 'ai' && (
            <div className="ai-generation-info">
              <p>🤖 <strong>AI-generering:</strong> Prosesstrinn vil bli generert automatisk basert på prosessinformasjonen når du oppretter prosessen.</p>
              <div className="ai-preview">
                <h4>AI vil generere trinn basert på:</h4>
                <ul>
                  <li>Prosesstittel: <strong>{formData.title || 'Ikke spesifisert'}</strong></li>
                  <li>Kategori: <strong>{formData.category || 'Ikke spesifisert'}</strong></li>
                  {formData.itilArea && <li>ITIL-område: <strong>{formData.itilArea}</strong></li>}
                  {selectedTemplate && <li>Mal: <strong>{selectedTemplate.name}</strong></li>}
                </ul>
              </div>
            </div>
          )}

          {stepBuilderMode === 'template' && selectedTemplate && (
            <div className="template-steps-preview">
              <h4>📋 Forhåndsdefinerte trinn fra {selectedTemplate.name}</h4>
              <div className="template-steps">
                {selectedTemplate.defaultSteps.map((step: any, index: number) => (
                  <div key={index} className="template-step">
                    <span className="step-number">{index + 1}</span>
                    <div className="step-content">
                      <h5>{step.title}</h5>
                      <p>{step.description}</p>
                      <div className="step-meta">
                        <span>👤 {step.responsible_role}</span>
                        <span>⏱️ {step.estimated_duration} min</span>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
              <p><small>💡 Disse trinnene vil bli brukt som utgangspunkt når prosessen opprettes.</small></p>
            </div>
          )}
        </div>

        {/* AI Process Suggestions */}
        <div className="form-section">
          <div className="ai-suggestions-toggle">
            <button
              type="button"
              className={`toggle-btn ${showAISuggestions ? 'active' : ''}`}
              onClick={() => setShowAISuggestions(!showAISuggestions)}
            >
              {showAISuggestions ? '🔽' : '▶️'} 🤖 AI-drevne prosessforslag {showAISuggestions ? '(skjul)' : '(vis)'}
            </button>
          </div>
          
          {showAISuggestions && (
            <AIProcessSuggestions
              processTitle={formData.title}
              processDescription={formData.description}
              category={formData.category}
              itilArea={formData.itilArea}
              currentSteps={processSteps}
              onApplySuggestion={handleApplyAISuggestion}
              onGenerateSteps={handleGenerateAISteps}
              isLoading={isLoading || generatingAISteps}
            />
          )}
        </div>

        {/* Error handling */}
        {error && (
          <div className="error-message">
            ❌ {error}
          </div>
        )}

        {/* Action buttons */}
        <div className="form-actions">
          <button
            type="button"
            onClick={handleCancel}
            className="btn-secondary"
            disabled={isLoading}
          >
            Avbryt
          </button>
          <button
            type="submit"
            className="btn-primary"
            disabled={isLoading || !formData.title.trim() || !formData.description.trim() || !formData.category}
          >
            {isLoading ? 'Oppretter...' : 'Opprett prosess'}
          </button>
        </div>

        {/* Progress indicator */}
        {isLoading && (
          <div className="progress-indicator">
            <div className="spinner"></div>
            <span>Oppretter ny prosess...</span>
          </div>
        )}
      </form>
    </div>
  );
};