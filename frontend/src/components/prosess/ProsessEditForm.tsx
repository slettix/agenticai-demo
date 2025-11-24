import React, { useState, useEffect } from 'react';
import { ProsessDetail, ProsessTagDto, ProsessStepDto, StepType } from '../../types/prosess';
import { ProcessStepBuilder } from './ProcessStepBuilder.tsx';

interface ProsessEditFormProps {
  prosess: ProsessDetail;
  onChange: (prosess: ProsessDetail) => void;
  disabled?: boolean;
}

export const ProsessEditForm: React.FC<ProsessEditFormProps> = ({
  prosess,
  onChange,
  disabled = false
}) => {
  const [formData, setFormData] = useState(prosess);
  const [newTagName, setNewTagName] = useState('');

  useEffect(() => {
    setFormData(prosess);
  }, [prosess]);

  const handleFieldChange = (field: keyof ProsessDetail, value: any) => {
    const updatedProsess = { ...formData, [field]: value };
    setFormData(updatedProsess);
    onChange(updatedProsess);
  };

  const handleAddTag = () => {
    if (!newTagName.trim()) return;

    const newTag: ProsessTagDto = {
      id: Date.now(), // Temporary ID for new tags
      name: newTagName.trim(),
      color: '#007bff'
    };

    const updatedTags = [...formData.tags, newTag];
    handleFieldChange('tags', updatedTags);
    setNewTagName('');
  };

  const handleRemoveTag = (tagId: number) => {
    const updatedTags = formData.tags.filter(tag => tag.id !== tagId);
    handleFieldChange('tags', updatedTags);
  };

  const handleStepsChange = (steps: ProsessStepDto[]) => {
    handleFieldChange('steps', steps);
  };

  const handleKeyPress = (e: React.KeyboardEvent, action: () => void) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      action();
    }
  };

  return (
    <div className="prosess-edit-form">
      <div className="form-section">
        <h3>Grunnleggende informasjon</h3>
        
        <div className="form-row">
          <div className="form-group">
            <label htmlFor="title">Prosessnavn *</label>
            <input
              id="title"
              type="text"
              value={formData.title}
              onChange={(e) => handleFieldChange('title', e.target.value)}
              disabled={disabled}
              required
              className="form-input"
            />
          </div>
          
          <div className="form-group">
            <label htmlFor="category">Kategori *</label>
            <select
              id="category"
              value={formData.category}
              onChange={(e) => handleFieldChange('category', e.target.value)}
              disabled={disabled}
              required
              className="form-select"
            >
              <option value="">Velg kategori</option>
              <option value="HR">HR</option>
              <option value="IT">IT</option>
              <option value="Økonomi">Økonomi</option>
              <option value="Drift">Drift</option>
              <option value="Kvalitet">Kvalitet</option>
              <option value="Compliance">Compliance</option>
              <option value="Markedsføring">Markedsføring</option>
              <option value="Salg">Salg</option>
              <option value="Kundeservice">Kundeservice</option>
              <option value="Generell">Generell</option>
            </select>
          </div>
        </div>

        <div className="form-group">
          <label htmlFor="description">Beskrivelse *</label>
          <textarea
            id="description"
            value={formData.description}
            onChange={(e) => handleFieldChange('description', e.target.value)}
            disabled={disabled}
            required
            rows={4}
            className="form-textarea"
            placeholder="Beskriv prosessen og dens formål..."
          />
        </div>
      </div>

      <div className="form-section">
        <h3>Tags</h3>
        
        <div className="tags-container">
          <div className="existing-tags">
            {formData.tags.map((tag) => (
              <div key={tag.id} className="tag-item" style={{ borderColor: tag.color }}>
                <span>{tag.name}</span>
                {!disabled && (
                  <button
                    type="button"
                    onClick={() => handleRemoveTag(tag.id)}
                    className="tag-remove"
                    aria-label={`Fjern tag ${tag.name}`}
                  >
                    ×
                  </button>
                )}
              </div>
            ))}
          </div>
          
          {!disabled && (
            <div className="add-tag-form">
              <input
                type="text"
                value={newTagName}
                onChange={(e) => setNewTagName(e.target.value)}
                onKeyPress={(e) => handleKeyPress(e, handleAddTag)}
                placeholder="Legg til tag..."
                className="tag-input"
              />
              <button
                type="button"
                onClick={handleAddTag}
                disabled={!newTagName.trim()}
                className="add-tag-button"
              >
                + Legg til
              </button>
            </div>
          )}
        </div>
      </div>

      <div className="form-section">
        <h3>Prosesstrinn</h3>
        <ProcessStepBuilder
          steps={formData.steps}
          onChange={handleStepsChange}
          disabled={disabled}
        />
      </div>

      <div className="form-section">
        <h3>Metadata</h3>
        
        <div className="metadata-grid">
          <div className="metadata-item">
            <label>Status:</label>
            <span className={`status-badge status-${formData.status}`}>
              {getStatusText(formData.status)}
            </span>
          </div>
          
          <div className="metadata-item">
            <label>Opprettet av:</label>
            <span>{formData.createdByUserName}</span>
          </div>
          
          <div className="metadata-item">
            <label>Opprettet:</label>
            <span>{new Date(formData.createdAt).toLocaleDateString('no-NO')}</span>
          </div>
          
          <div className="metadata-item">
            <label>Sist endret:</label>
            <span>{new Date(formData.updatedAt).toLocaleDateString('no-NO')}</span>
          </div>
          
          {formData.ownerName && (
            <div className="metadata-item">
              <label>Eier:</label>
              <span>{formData.ownerName}</span>
            </div>
          )}
          
          <div className="metadata-item">
            <label>Visninger:</label>
            <span>{formData.viewCount}</span>
          </div>
        </div>
      </div>
    </div>
  );
};

function getStatusText(status: number): string {
  switch (status) {
    case 0: return 'Utkast';
    case 1: return 'Venter godkjenning';
    case 2: return 'Under godkjenning';
    case 3: return 'Godkjent';
    case 4: return 'Avvist';
    case 5: return 'Publisert';
    case 6: return 'Utdatert';
    case 7: return 'Arkivert';
    default: return 'Ukjent';
  }
}