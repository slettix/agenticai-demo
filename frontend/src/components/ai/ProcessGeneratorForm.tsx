import React, { useState } from 'react';
import { ProcessGenerationRequest } from '../../types/agent.ts';
import { agentService } from '../../services/agentService.ts';

interface ProcessGeneratorFormProps {
  onJobSubmitted: (jobId: string) => void;
  onError: (error: string) => void;
}

export const ProcessGeneratorForm: React.FC<ProcessGeneratorFormProps> = ({ onJobSubmitted, onError }) => {
  const [formData, setFormData] = useState<ProcessGenerationRequest>({
    title: '',
    description: '',
    category: '',
    requirements: [],
    targetAudience: '',
    complexityLevel: 'medium'
  });
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [requirementInput, setRequirementInput] = useState('');

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const addRequirement = () => {
    if (requirementInput.trim()) {
      setFormData(prev => ({
        ...prev,
        requirements: [...(prev.requirements || []), requirementInput.trim()]
      }));
      setRequirementInput('');
    }
  };

  const removeRequirement = (index: number) => {
    setFormData(prev => ({
      ...prev,
      requirements: prev.requirements?.filter((_, i) => i !== index) || []
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);

    try {
      const response = await agentService.generateProcess(formData);
      onJobSubmitted(response.jobId);
      
      // Reset form
      setFormData({
        title: '',
        description: '',
        category: '',
        requirements: [],
        targetAudience: '',
        complexityLevel: 'medium'
      });
    } catch (error) {
      onError(error instanceof Error ? error.message : 'Failed to submit generation request');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="ai-form-container">
      <h2>🤖 AI Process Generator</h2>
      <p>Beskrive prosessen du ønsker å generere, så lager AI-en et forslag for deg.</p>
      
      <form onSubmit={handleSubmit} className="ai-form">
        <div className="form-group">
          <label htmlFor="title">Prosess tittel*</label>
          <input
            type="text"
            id="title"
            name="title"
            value={formData.title}
            onChange={handleInputChange}
            placeholder="F.eks. 'Ansettelsesprosess for nye medarbeidere'"
            required
          />
        </div>

        <div className="form-group">
          <label htmlFor="description">Beskrivelse*</label>
          <textarea
            id="description"
            name="description"
            value={formData.description}
            onChange={handleInputChange}
            placeholder="Beskriv hva prosessen skal oppnå og hovedaktivitetene..."
            rows={4}
            required
          />
        </div>

        <div className="form-row">
          <div className="form-group">
            <label htmlFor="category">Kategori*</label>
            <select
              id="category"
              name="category"
              value={formData.category}
              onChange={handleInputChange}
              required
            >
              <option value="">Velg kategori</option>
              <option value="HR">HR</option>
              <option value="Finance">Økonomi</option>
              <option value="IT">IT</option>
              <option value="Operations">Drift</option>
              <option value="Sales">Salg</option>
              <option value="Marketing">Markedsføring</option>
              <option value="Legal">Juridisk</option>
              <option value="Other">Annet</option>
            </select>
          </div>

          <div className="form-group">
            <label htmlFor="complexityLevel">Kompleksitet</label>
            <select
              id="complexityLevel"
              name="complexityLevel"
              value={formData.complexityLevel}
              onChange={handleInputChange}
            >
              <option value="simple">Enkel</option>
              <option value="medium">Middels</option>
              <option value="complex">Kompleks</option>
            </select>
          </div>
        </div>

        <div className="form-group">
          <label htmlFor="targetAudience">Målgruppe</label>
          <input
            type="text"
            id="targetAudience"
            name="targetAudience"
            value={formData.targetAudience}
            onChange={handleInputChange}
            placeholder="Hvem skal bruke denne prosessen?"
          />
        </div>

        <div className="form-group">
          <label>Krav og spesifikasjoner</label>
          <div className="requirements-input">
            <input
              type="text"
              value={requirementInput}
              onChange={(e) => setRequirementInput(e.target.value)}
              placeholder="Legg til et krav..."
              onKeyPress={(e) => e.key === 'Enter' && (e.preventDefault(), addRequirement())}
            />
            <button type="button" onClick={addRequirement} className="add-button">
              + Legg til
            </button>
          </div>
          
          {formData.requirements && formData.requirements.length > 0 && (
            <div className="requirements-list">
              {formData.requirements.map((req, index) => (
                <div key={index} className="requirement-item">
                  <span>{req}</span>
                  <button 
                    type="button" 
                    onClick={() => removeRequirement(index)}
                    className="remove-button"
                  >
                    ×
                  </button>
                </div>
              ))}
            </div>
          )}
        </div>

        <div className="form-actions">
          <button 
            type="submit" 
            disabled={isSubmitting || !formData.title || !formData.description || !formData.category}
            className="submit-button ai-submit"
          >
            {isSubmitting ? '🤖 Genererer...' : '🚀 Generer prosess'}
          </button>
        </div>
      </form>
    </div>
  );
};