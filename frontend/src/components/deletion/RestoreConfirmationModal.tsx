import React, { useState } from 'react';
import { DeletedProsessDto } from '../../types/deletion.ts';
import './deletion.css';

interface RestoreConfirmationModalProps {
  prosess: DeletedProsessDto;
  isOpen: boolean;
  onClose: () => void;
  onConfirm: (reason: string) => void;
}

export const RestoreConfirmationModal: React.FC<RestoreConfirmationModalProps> = ({
  prosess,
  isOpen,
  onClose,
  onConfirm
}) => {
  const [reason, setReason] = useState('');

  if (!isOpen) return null;

  const handleConfirm = () => {
    if (!reason.trim()) {
      alert('Vennligst oppgi en grunn for gjenopprettingen');
      return;
    }

    onConfirm(reason);
  };

  const formatDate = (dateString: string): string => {
    return new Date(dateString).toLocaleDateString('nb-NO', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  return (
    <div className="modal-overlay">
      <div className="restore-modal">
        <div className="modal-header">
          <h2>↩️ Gjenopprett prosess</h2>
          <button onClick={onClose} className="close-btn">×</button>
        </div>

        <div className="modal-body">
          <div className="prosess-info">
            <h3>{prosess.title}</h3>
            <p>{prosess.description}</p>
            <div className="prosess-meta">
              <span className="category">📁 {prosess.category}</span>
              <span className="deleted-info">
                🗑️ Slettet {formatDate(prosess.deletedAt)} av {prosess.deletedByUser}
              </span>
            </div>
            {prosess.reason && (
              <div className="deletion-reason">
                <strong>Opprinnelig slettingsgrunn:</strong> {prosess.reason}
              </div>
            )}
          </div>

          <div className="info-box">
            <h4>🔄 Hva skjer ved gjenoppretting?</h4>
            <ul>
              <li>Prosessen vil bli synlig i søkeresultater igjen</li>
              <li>Status vil bli satt til "Utkast"</li>
              <li>Alle historiske data og versjoner bevares</li>
              <li>Brukere kan få tilgang til prosessen igjen</li>
            </ul>
          </div>

          <div className="reason-section">
            <label htmlFor="restore-reason">
              <strong>Grunn for gjenoppretting *</strong>
            </label>
            <textarea
              id="restore-reason"
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              placeholder="Beskriv hvorfor prosessen gjenopprettes (f.eks. feilaktig slettet, trengs igjen, etc.)"
              rows={4}
              className="reason-textarea"
              required
            />
          </div>

          <div className="action-summary">
            <h4>Sammendrag av handling:</h4>
            <ul>
              <li>Prosess: <strong>{prosess.title}</strong></li>
              <li>Handling: <strong>Gjenopprett fra sletting</strong></li>
              <li>Ny status: <strong>Utkast</strong></li>
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
            className={`btn-primary ${!reason.trim() ? 'disabled' : ''}`}
            disabled={!reason.trim()}
          >
            ↩️ Gjenopprett prosess
          </button>
        </div>
      </div>
    </div>
  );
};