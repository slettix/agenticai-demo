import React, { useState, useEffect } from 'react';
import { useAuth } from '../../contexts/AuthContext';
import { prosessService } from '../../services/prosessService';
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
        tags: formData.tags ? formData.tags.split(',').map(t => t.trim()).filter(t => t) : undefined
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
    return formData.title || formData.description || formData.category || formData.itilArea || formData.tags;
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