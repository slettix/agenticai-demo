import React, { useState } from 'react';
import { ProsessDetailDto } from '../../types/prosess.ts';
import './deletion.css';

interface DeleteConfirmationModalProps {
  prosess: ProsessDetailDto;
  isOpen: boolean;
  onClose: () => void;
  onConfirm: (reason: string, forceDelete: boolean) => void;
  hasActiveInstances: boolean;
}

export const DeleteConfirmationModal: React.FC<DeleteConfirmationModalProps> = ({
  prosess,
  isOpen,
  onClose,
  onConfirm,
  hasActiveInstances
}) => {
  const [reason, setReason] = useState('');
  const [forceDelete, setForceDelete] = useState(false);
  const [deleteType, setDeleteType] = useState<'soft' | 'archive'>('soft');

  if (!isOpen) return null;

  const handleConfirm = () => {
    if (!reason.trim()) {
      alert('Vennligst oppgi en grunn for slettingen');
      return;
    }

    onConfirm(reason, forceDelete);
  };

  const getWarningMessage = () => {
    if (hasActiveInstances) {
      return {
        level: 'error',
        message: '⚠️ ADVARSEL: Denne prosessen har aktive instanser som kjører. Sletting kan påvirke pågående arbeid.'
      };
    }
    
    return {
      level: 'warning',
      message: '⚠️ Denne handlingen vil fjerne prosessen fra søkeresultater og gjøre den utilgjengelig for brukere.'
    };
  };

  const warning = getWarningMessage();

  return (
    <div className="modal-overlay">
      <div className="deletion-modal">
        <div className="modal-header">
          <h2>🗑️ Slett prosess</h2>
          <button onClick={onClose} className="close-btn">×</button>
        </div>

        <div className="modal-body">
          <div className="prosess-info">
            <h3>{prosess.title}</h3>
            <p>{prosess.description}</p>
            <div className="prosess-meta">
              <span className="category">📁 {prosess.category}</span>
              <span className="views">👁 {prosess.viewCount} visninger</span>
            </div>
          </div>

          <div className={`warning-box ${warning.level}`}>
            <p>{warning.message}</p>
          </div>

          <div className="deletion-options">
            <h4>Slettingstype:</h4>
            <div className="radio-group">
              <label>
                <input 
                  type="radio" 
                  value="soft" 
                  checked={deleteType === 'soft'} 
                  onChange={(e) => setDeleteType(e.target.value as 'soft')}
                />
                <span className="radio-label">
                  <strong>Soft delete</strong> - Skjul fra brukere men behold i database
                </span>
              </label>
              <label>
                <input 
                  type="radio" 
                  value="archive" 
                  checked={deleteType === 'archive'} 
                  onChange={(e) => setDeleteType(e.target.value as 'archive')}
                />
                <span className="radio-label">
                  <strong>Arkiver</strong> - Marker som arkivert men la brukere se den
                </span>
              </label>
            </div>
          </div>

          {hasActiveInstances && (
            <div className="force-delete-section">
              <label className="checkbox-label">
                <input 
                  type="checkbox" 
                  checked={forceDelete} 
                  onChange={(e) => setForceDelete(e.target.checked)}
                />
                <span>Tving sletting selv med aktive instanser</span>
              </label>
              <p className="force-delete-warning">
                ⚠️ Dette kan forårsake problemer for pågående arbeid
              </p>
            </div>
          )}

          <div className="reason-section">
            <label htmlFor="deletion-reason">
              <strong>Grunn for sletting *</strong>
            </label>
            <textarea
              id="deletion-reason"
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              placeholder="Beskriv hvorfor prosessen slettes (f.eks. utdatert, erstattet av ny versjon, feil, etc.)"
              rows={4}
              className="reason-textarea"
              required
            />
          </div>

          <div className="action-summary">
            <h4>Sammendrag av handling:</h4>
            <ul>
              <li>Prosess: <strong>{prosess.title}</strong></li>
              <li>Type: <strong>{deleteType === 'soft' ? 'Soft delete' : 'Arkivering'}</strong></li>
              {hasActiveInstances && forceDelete && (
                <li className="warning-item">⚠️ Tving sletting med aktive instanser</li>
              )}
              <li>Grunn: <em>{reason || 'Ikke oppgitt'}</em></li>
            </ul>
          </div>
        </div>

        <div className="modal-footer">
          <button onClick={onClose} className="btn-secondary">
            Avbryt
          </button>
          <button 
            onClick={handleConfirm} 
            className={`btn-danger ${!reason.trim() ? 'disabled' : ''}`}
            disabled={!reason.trim() || (hasActiveInstances && !forceDelete)}
          >
            {deleteType === 'soft' ? '🗑️ Slett prosess' : '📦 Arkiver prosess'}
          </button>
        </div>
      </div>
    </div>
  );
};