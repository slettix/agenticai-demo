import React, { useState } from 'react';
import { ProcessRevisionRequest } from '../../types/agent.ts';
import { agentService } from '../../services/agentService.ts';

interface ProcessRevisionFormProps {
  processId: number;
  processTitle: string;
  onJobSubmitted: (jobId: string) => void;
  onError: (error: string) => void;
}

export const ProcessRevisionForm: React.FC<ProcessRevisionFormProps> = ({ 
  processId, 
  processTitle, 
  onJobSubmitted, 
  onError 
}) => {
  const [formData, setFormData] = useState<ProcessRevisionRequest>({
    processId,
    revisionType: 'optimize',
    feedback: [],
    improvementGoals: [],
    customInstructions: ''
  });
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [feedbackInput, setFeedbackInput] = useState('');
  const [goalInput, setGoalInput] = useState('');

  const handleInputChange = (e: React.ChangeEvent<HTMLSelectElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const addFeedback = () => {
    if (feedbackInput.trim()) {
      setFormData(prev => ({
        ...prev,
        feedback: [...(prev.feedback || []), feedbackInput.trim()]
      }));
      setFeedbackInput('');
    }
  };

  const removeFeedback = (index: number) => {
    setFormData(prev => ({
      ...prev,
      feedback: prev.feedback?.filter((_, i) => i !== index) || []
    }));
  };

  const addGoal = () => {
    if (goalInput.trim()) {
      setFormData(prev => ({
        ...prev,
        improvementGoals: [...(prev.improvementGoals || []), goalInput.trim()]
      }));
      setGoalInput('');
    }
  };

  const removeGoal = (index: number) => {
    setFormData(prev => ({
      ...prev,
      improvementGoals: prev.improvementGoals?.filter((_, i) => i !== index) || []
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);

    try {
      const response = await agentService.reviseProcess(formData);
      onJobSubmitted(response.jobId);
    } catch (error) {
      onError(error instanceof Error ? error.message : 'Failed to submit revision request');
    } finally {
      setIsSubmitting(false);
    }
  };

  const revisionTypeDescriptions = {
    optimize: 'Optimalisere for effektivitet og hastighet',
    simplify: 'Forenkle og gjøre mer brukervennlig',
    expand: 'Utvide med mer detaljer og kontroller',
    custom: 'Egendefinerte endringer'
  };

  return (
    <div className="ai-form-container">
      <h2>🔧 AI Prosessrevisjon</h2>
      <p>Få AI til å forbedre prosessen <strong>"{processTitle}"</strong> basert på tilbakemeldinger og mål.</p>
      
      <form onSubmit={handleSubmit} className="ai-form">
        <div className="form-group">
          <label htmlFor="revisionType">Type revisjon*</label>
          <select
            id="revisionType"
            name="revisionType"
            value={formData.revisionType}
            onChange={handleInputChange}
            required
          >
            {Object.entries(revisionTypeDescriptions).map(([value, description]) => (
              <option key={value} value={value}>
                {description}
              </option>
            ))}
          </select>
        </div>

        <div className="form-group">
          <label>Tilbakemeldinger fra brukere</label>
          <div className="feedback-input">
            <input
              type="text"
              value={feedbackInput}
              onChange={(e) => setFeedbackInput(e.target.value)}
              placeholder="F.eks. 'Prosessen tar for lang tid'"
              onKeyPress={(e) => e.key === 'Enter' && (e.preventDefault(), addFeedback())}
            />
            <button type="button" onClick={addFeedback} className="add-button">
              + Legg til
            </button>
          </div>
          
          {formData.feedback && formData.feedback.length > 0 && (
            <div className="feedback-list">
              {formData.feedback.map((feedback, index) => (
                <div key={index} className="feedback-item">
                  <span>💬 {feedback}</span>
                  <button 
                    type="button" 
                    onClick={() => removeFeedback(index)}
                    className="remove-button"
                  >
                    ×
                  </button>
                </div>
              ))}
            </div>
          )}
        </div>

        <div className="form-group">
          <label>Forbedringsmål</label>
          <div className="goals-input">
            <input
              type="text"
              value={goalInput}
              onChange={(e) => setGoalInput(e.target.value)}
              placeholder="F.eks. 'Reduser tiden med 30%'"
              onKeyPress={(e) => e.key === 'Enter' && (e.preventDefault(), addGoal())}
            />
            <button type="button" onClick={addGoal} className="add-button">
              + Legg til
            </button>
          </div>
          
          {formData.improvementGoals && formData.improvementGoals.length > 0 && (
            <div className="goals-list">
              {formData.improvementGoals.map((goal, index) => (
                <div key={index} className="goal-item">
                  <span>🎯 {goal}</span>
                  <button 
                    type="button" 
                    onClick={() => removeGoal(index)}
                    className="remove-button"
                  >
                    ×
                  </button>
                </div>
              ))}
            </div>
          )}
        </div>

        {formData.revisionType === 'custom' && (
          <div className="form-group">
            <label htmlFor="customInstructions">Egendefinerte instruksjoner</label>
            <textarea
              id="customInstructions"
              name="customInstructions"
              value={formData.customInstructions}
              onChange={handleInputChange}
              placeholder="Beskriv spesifikke endringer du ønsker..."
              rows={4}
            />
          </div>
        )}

        <div className="form-actions">
          <button 
            type="submit" 
            disabled={isSubmitting}
            className="submit-button ai-submit"
          >
            {isSubmitting ? '🔧 Reviderer...' : '🚀 Start revisjon'}
          </button>
        </div>
      </form>
    </div>
  );
};