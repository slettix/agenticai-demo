import React, { useState } from 'react';
import { SubmitForApprovalRequest } from '../../types/approval.ts';
import { ProsessDetail } from '../../types/prosess.ts';

interface SubmitForApprovalModalProps {
  prosess: ProsessDetail;
  onSubmit: (request: SubmitForApprovalRequest) => void;
  onCancel: () => void;
  loading: boolean;
}

export const SubmitForApprovalModal: React.FC<SubmitForApprovalModalProps> = ({
  prosess,
  onSubmit,
  onCancel,
  loading
}) => {
  const [comment, setComment] = useState('');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    onSubmit({
      requestComment: comment.trim() || undefined
    });
  };

  return (
    <div className="modal-overlay">
      <div className="modal-content submit-approval-modal">
        <h2>Send til godkjenning</h2>
        
        <div className="process-summary">
          <h3>{prosess.title}</h3>
          <p><strong>Kategori:</strong> {prosess.category}</p>
          <p><strong>Status:</strong> Utkast</p>
          <p><strong>Beskrivelse:</strong> {prosess.description}</p>
        </div>

        <div className="approval-info">
          <h4>Hva skjer når du sender til godkjenning?</h4>
          <ul>
            <li>Prosessen vil få status "Venter på godkjenning"</li>
            <li>Godkjennere vil få varsel om den nye forespørselen</li>
            <li>Du kan ikke redigere prosessen mens den er under godkjenning</li>
            <li>Du vil få beskjed når prosessen er godkjent eller avvist</li>
          </ul>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="comment-section">
            <label htmlFor="approval-comment">
              Kommentar til godkjennere (valgfri):
            </label>
            <textarea
              id="approval-comment"
              value={comment}
              onChange={(e) => setComment(e.target.value)}
              placeholder="Legg til kontekst, spesielle merknader eller spørsmål til godkjennerne..."
              rows={4}
              disabled={loading}
            />
          </div>

          <div className="modal-actions">
            <button
              type="button"
              onClick={onCancel}
              className="cancel-button"
              disabled={loading}
            >
              Avbryt
            </button>
            <button
              type="submit"
              className="submit-button approve"
              disabled={loading}
            >
              {loading ? (
                <>
                  <span className="spinner-small"></span>
                  Sender...
                </>
              ) : (
                '📤 Send til godkjenning'
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};