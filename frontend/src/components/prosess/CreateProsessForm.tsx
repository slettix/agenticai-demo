import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { prosessService } from '../../services/prosessService.ts';
import { agentService } from '../../services/agentService.ts';
import type { ProcessGenerationRequest, AgentJobStatus, ProcessGenerationResult } from '../../types/agent.ts';
import type { CreateProsessStepRequest } from '../../types/prosess.ts';
import { StepType } from '../../types/prosess.ts';
import { ProcessStepBuilder, ProcessStep } from './ProcessStepBuilder.tsx';
import { AIProcessSuggestions } from './AIProcessSuggestions.tsx';
import { ProcessTemplateManager } from './ProcessTemplateManager.tsx';
import './CreateProsessForm.css';

interface CreateProsessFormProps {}

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

// Helper function to convert numeric step type to StepType enum
const convertStepType = (numericType: number): StepType => {
  switch (numericType) {
    case 0: return StepType.Start;
    case 1: return StepType.Task;
    case 2: return StepType.Decision;
    case 3: return StepType.Document;
    case 4: return StepType.Approval;
    case 5: return StepType.Gateway;
    case 6: return StepType.Review;
    case 7: return StepType.Wait;
    case 8: return StepType.End;
    case 9: return StepType.Subprocess;
    default: return StepType.Task; // Default fallback
  }
};

export const CreateProsessForm: React.FC<CreateProsessFormProps> = () => {
  const navigate = useNavigate();
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
  const [aiJobId, setAiJobId] = useState<string | null>(null);
  const [aiJobStatus, setAiJobStatus] = useState<AgentJobStatus | null>(null);
  const [aiGenerationResult, setAiGenerationResult] = useState<ProcessGenerationResult | null>(null);
  const [aiProgress, setAiProgress] = useState(0);
  const [showTemplateManager, setShowTemplateManager] = useState(false);
  const [selectedProcessTemplate, setSelectedProcessTemplate] = useState<any>(null);
  
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
      console.log('Categories data received:', categoriesData);
      setCategories(categoriesData);
    } catch (err) {
      console.error('Error loading categories:', err);
      setError('Kunne ikke laste kategorier');
    }
  };

  const loadTemplatesForArea = async (area: string) => {
    try {
      console.log('Loading templates for area:', area);
      const templatesData = await prosessService.getITILTemplates(area);
      console.log('Templates data received:', templatesData);
      setTemplates(templatesData || []);
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

      // Convert ProcessStep[] to CreateProsessStepRequest[]
      const convertedSteps: CreateProsessStepRequest[] | undefined = processSteps.length > 0 ? processSteps.map(step => {
        console.log('Converting step:', step);
        const stepType = typeof step.type === 'number' ? convertStepType(step.type) : step.type;
        console.log(`Step type conversion: ${step.type} -> ${stepType}`);
        return {
          title: step.title,
          description: step.description,
          type: stepType,
          responsibleRole: step.responsibleRole || undefined,
          estimatedDurationMinutes: step.estimatedDuration || undefined,
          orderIndex: step.orderIndex || 0,
          isOptional: step.isOptional || false,
          detailedInstructions: step.detailedInstructions || undefined,
          iTILGuidance: step.itilGuidance || undefined
        };
      }) : undefined;

      // Match the exact CreateProsessRequest DTO structure
      const requestData = {
        title: formData.title.trim(),
        description: formData.description.trim(),
        category: formData.category,
        itilArea: formData.itilArea || undefined,
        priority: formData.priority,
        tags: formData.tags && formData.tags.trim() ? formData.tags.split(',').map(t => t.trim()).filter(t => t) : undefined,
        processSteps: convertedSteps
      };

      console.log('Sending request to backend:', requestData);
      console.log('Process steps being sent:', convertedSteps);

      console.log('Calling prosessService.createProsess...');
      const newProsess = await prosessService.createProsess(requestData);
      console.log('Response from backend:', newProsess);
      
      if (newProsess && newProsess.id) {
        console.log('Process created successfully, navigating to process ID:', newProsess.id);
        navigate(`/prosess/${newProsess.id}`);
      } else {
        throw new Error('Prosess ble opprettet, men ID mangler i responsen');
      }
    } catch (err) {
      console.error('Error in handleSubmit:', err);
      setError(err instanceof Error ? err.message : 'En feil oppstod under opprettelse av prosess');
    } finally {
      setIsLoading(false);
    }
  };

  const handleCancel = () => {
    if (hasUnsavedChanges()) {
      if (window.confirm('Du har ulagrede endringer. Er du sikker på at du vil avbryte?')) {
        navigate('/prosesser');
      }
    } else {
      navigate('/prosesser');
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
    if (!formData.title.trim() || !formData.description.trim()) {
      setError('Tittel og beskrivelse må være fylt ut for AI-generering');
      return;
    }

    setGeneratingAISteps(true);
    setStepBuilderMode('ai');
    setError(null);
    setAiProgress(0);
    
    try {
      // Create AI generation request
      const request: ProcessGenerationRequest = {
        title: formData.title.trim(),
        description: formData.description.trim(),
        category: formData.category || 'ITSM',
        requirements: formData.itilArea ? [`ITIL ${formData.itilArea} compliance`] : [],
        targetAudience: 'IT Service Management teams',
        complexityLevel: 'medium'
      };

      console.log('Sending AI generation request:', request);
      
      // Submit AI generation job
      const jobResponse = await agentService.generateProcess(request);
      setAiJobId(jobResponse.jobId);
      setAiProgress(10);

      console.log('AI job started:', jobResponse);
      
      // Poll for job completion
      const finalStatus = await agentService.pollJobStatus(
        jobResponse.jobId,
        (status) => {
          setAiJobStatus(status);
          setAiProgress(status.progress || 0);
          console.log('AI job progress:', status);
        }
      );

      if (finalStatus.status === 'completed') {
        // Get the generation result
        const result = await agentService.getGenerationResult(jobResponse.jobId);
        setAiGenerationResult(result);
        
        // Convert AI steps to our ProcessStep format
        const convertedSteps = (result.steps || []).map((step, index) => {
          console.log(`Processing AI step ${index + 1}:`, step);
          return {
            id: `ai_${Date.now()}_${index}`,
            title: step.title || `Step ${index + 1}`,
            description: step.description || '',
            type: typeof step.type === 'number' ? step.type : parseInt(step.type) || 1, // Ensure numeric type
            responsibleRole: step.responsible_role || step.responsibleRole || 'Assignee',
            estimatedDuration: parseInt(step.estimated_duration || step.estimatedDuration || 30),
            orderIndex: parseInt(step.order_index || step.orderIndex || index),
            isOptional: Boolean(step.is_optional || step.isOptional || false),
            detailedInstructions: step.detailed_instructions || step.detailedInstructions || '',
            itilGuidance: 'AI-generated step based on ITIL best practices'
          };
        });

        setProcessSteps(convertedSteps);
        setAiProgress(100);
        
        console.log('AI generation completed:', result);
        alert(`✅ AI har generert ${result.steps?.length || 0} prosesstrinn!\n\nDu kan nå redigere trinnene eller opprette prosessen.`);
        
      } else if (finalStatus.status === 'failed') {
        throw new Error(finalStatus.errorMessage || 'AI-generering feilet');
      }
      
    } catch (error) {
      console.error('AI generation error:', error);
      setError(`AI-generering feilet: ${error instanceof Error ? error.message : 'Ukjent feil'}`);
      setStepBuilderMode('manual'); // Fall back to manual mode
    } finally {
      setGeneratingAISteps(false);
    }
  };

  const handleProcessTemplateSelect = (template: any) => {
    setSelectedProcessTemplate(template);
    
    // Auto-fill form data from template
    setFormData(prev => ({
      ...prev,
      title: template.name,
      description: template.description,
      category: template.category,
      itilArea: template.itilArea
    }));
    
    // Load template steps
    setProcessSteps(template.defaultSteps);
    setStepBuilderMode('template');
    
    // Close template manager
    setShowTemplateManager(false);
    
    alert(`Mal "${template.name}" er valgt og lastet inn i skjemaet.`);
  };

  const handleValidateCompliance = (process: any) => {
    // Mock compliance validation
    return {
      score: 85,
      status: 'partial' as const,
      passedChecks: [],
      failedChecks: [],
      recommendations: ['Legg til ekstra godkjenningstrinn', 'Inkluder GDPR-sjekk']
    };
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
                  {categories?.businessCategories?.map(cat => (
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
                {categories?.priorities?.map(priority => (
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
              {categories?.itilAreas?.map(area => (
                <option key={area.name} value={area.name}>{area.name}</option>
              ))}
            </select>
            {formData.itilArea && (
              <small className="field-help">
                {categories?.itilAreas?.find(a => a.name === formData.itilArea)?.description}
              </small>
            )}
          </div>

          {/* ITIL-maler */}
          {templates && templates.length > 0 && (
            <div className="form-group">
              <label>ITIL-prosessmaler</label>
              <div className="template-grid">
                {templates?.map(template => (
                  <div
                    key={template.name}
                    className={`template-card ${selectedTemplate?.name === template.name ? 'selected' : ''}`}
                    onClick={() => handleTemplateSelect(template)}
                  >
                    <h4>{template.name}</h4>
                    <p>{template.purpose}</p>
                    <div className="template-meta">
                      <span>📋 {template.defaultSteps?.length || 0} steg</span>
                      <span>📊 {template.kpis?.length || 0} KPI-er</span>
                    </div>
                  </div>
                ))}
              </div>
              {selectedTemplate && showTemplatePreview && (
                <div className="template-preview">
                  <h4>📋 Forhåndsvisning: {selectedTemplate.name}</h4>
                  <p><strong>Formål:</strong> {selectedTemplate.purpose}</p>
                  <p><strong>Nøkkelaktiviteter:</strong> {selectedTemplate.keyActivities?.join(', ') || 'Ikke spesifisert'}</p>
                  <p><strong>KPI-er:</strong> {selectedTemplate.kpis?.join(', ') || 'Ikke spesifisert'}</p>
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
              {!generatingAISteps && processSteps.length === 0 && (
                <>
                  <p>🤖 <strong>AI-generering:</strong> Klikk "Generer AI-trinn" for å la AI lage prosesstrinn automatisk basert på din beskrivelse.</p>
                  <div className="ai-preview">
                    <h4>AI vil generere trinn basert på:</h4>
                    <ul>
                      <li>Prosesstittel: <strong>{formData.title || 'Ikke spesifisert'}</strong></li>
                      <li>Beskrivelse: <strong>{formData.description ? formData.description.substring(0, 50) + '...' : 'Ikke spesifisert'}</strong></li>
                      <li>Kategori: <strong>{formData.category || 'Ikke spesifisert'}</strong></li>
                      {formData.itilArea && <li>ITIL-område: <strong>{formData.itilArea}</strong></li>}
                      <li>Estimert antall trinn: <strong>5-12 trinn</strong></li>
                    </ul>
                  </div>
                  <div className="ai-actions">
                    <button 
                      type="button"
                      onClick={handleGenerateAISteps}
                      disabled={!formData.title.trim() || !formData.description.trim() || generatingAISteps}
                      className="btn-generate-ai"
                    >
                      {generatingAISteps ? 'Genererer...' : '🚀 Generer AI-trinn'}
                    </button>
                    {(!formData.title.trim() || !formData.description.trim()) && (
                      <p className="requirement-note">* Prosesstittel og beskrivelse må være fylt ut</p>
                    )}
                  </div>
                </>
              )}
              
              {generatingAISteps && (
                <div className="ai-progress">
                  <h4>🤖 AI genererer prosesstrinn...</h4>
                  <div className="progress-bar">
                    <div className="progress-fill" style={{ width: `${aiProgress}%` }}></div>
                  </div>
                  <p>{aiJobStatus?.message || 'Behandler prosessinformasjon...'}</p>
                  <div className="progress-details">
                    {aiJobStatus && (
                      <>
                        <span>Status: {aiJobStatus.status}</span>
                        <span>Fremgang: {aiProgress}%</span>
                      </>
                    )}
                  </div>
                </div>
              )}
              
              {processSteps.length > 0 && !generatingAISteps && (
                <div className="ai-result">
                  <h4>✅ AI har generert {processSteps.length} prosesstrinn</h4>
                  <p>Du kan redigere trinnene nedenfor eller opprette prosessen direkte.</p>
                  <div className="ai-steps-preview">
                    {processSteps.map((step, index) => (
                      <div key={step.id} className="ai-step-card">
                        <div className="step-header">
                          <span className="step-number">{index + 1}</span>
                          <h5>{step.title}</h5>
                          <span className="step-type">{step.type}</span>
                        </div>
                        <p>{step.description}</p>
                        <div className="step-meta">
                          <span>👤 {step.responsibleRole}</span>
                          <span>⏱️ {step.estimatedDuration} min</span>
                        </div>
                      </div>
                    ))}
                  </div>
                  <div className="ai-result-actions">
                    <button 
                      type="button"
                      onClick={() => setStepBuilderMode('manual')}
                      className="btn-edit-steps"
                    >
                      ✏️ Rediger trinn
                    </button>
                    <button 
                      type="button"
                      onClick={handleGenerateAISteps}
                      className="btn-regenerate"
                    >
                      🔄 Generer på nytt
                    </button>
                  </div>
                </div>
              )}
            </div>
          )}

          {stepBuilderMode === 'template' && selectedTemplate && (
            <div className="template-steps-preview">
              <h4>📋 Forhåndsdefinerte trinn fra {selectedTemplate.name}</h4>
              <div className="template-steps">
                {selectedTemplate?.defaultSteps?.map((step: any, index: number) => (
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

        {/* Process Template Manager */}
        <div className="form-section">
          <div className="template-manager-toggle">
            <button
              type="button"
              className={`toggle-btn ${showTemplateManager ? 'active' : ''}`}
              onClick={() => setShowTemplateManager(!showTemplateManager)}
            >
              {showTemplateManager ? '🔽' : '▶️'} 📋 Prosessmaler og ITIL-samsvar {showTemplateManager ? '(skjul)' : '(vis)'}
            </button>
          </div>
          
          {showTemplateManager && (
            <ProcessTemplateManager
              itilArea={formData.itilArea}
              category={formData.category}
              onTemplateSelect={handleProcessTemplateSelect}
              onValidateCompliance={handleValidateCompliance}
              selectedTemplate={selectedProcessTemplate}
            />
          )}
        </div>

        {/* Selected Template Info */}
        {selectedProcessTemplate && (
          <div className="form-section">
            <div className="selected-template-info">
              <h3>📋 Valgt prosessmal</h3>
              <div className="template-summary">
                <h4>{selectedProcessTemplate.name}</h4>
                <p>{selectedProcessTemplate.description}</p>
                <div className="template-details">
                  <span>🎯 ITIL: {selectedProcessTemplate.itilArea}</span>
                  <span>📊 {selectedProcessTemplate.defaultSteps?.length || 0} trinn</span>
                  <span>⏱️ {selectedProcessTemplate.estimatedDuration} min</span>
                  <span>🏆 {selectedProcessTemplate.maturityLevel}</span>
                </div>
                <div className="template-actions">
                  <button
                    type="button"
                    className="remove-template-btn"
                    onClick={() => {
                      setSelectedProcessTemplate(null);
                      setProcessSteps([]);
                      setStepBuilderMode('manual');
                    }}
                  >
                    🗑️ Fjern mal
                  </button>
                  <button
                    type="button"
                    className="validate-compliance-btn"
                    onClick={() => {
                      const result = handleValidateCompliance(selectedProcessTemplate);
                      alert(`Samsvarsscore: ${result.score}%\nStatus: ${result.status}`);
                    }}
                  >
                    ✅ Sjekk samsvar
                  </button>
                </div>
              </div>
            </div>
          </div>
        )}

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