import React, { useState } from 'react';
import { ProsessApprovalRequest, ApprovalDecisionRequest } from '../../types/approval.ts';

interface ApprovalDecisionModalProps {
  request: ProsessApprovalRequest;
  onDecision: (decision: ApprovalDecisionRequest) => void;
  onCancel: () => void;
  loading: boolean;
}

export const ApprovalDecisionModal: React.FC<ApprovalDecisionModalProps> = ({
  request,
  onDecision,
  onCancel,
  loading
}) => {
  const [decision, setDecision] = useState<'approve' | 'reject' | null>(null);
  const [comment, setComment] = useState('');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (!decision) return;

    if (decision === 'reject' && !comment.trim()) {
      alert('Kommentar er påkrevd når en prosess avvises');
      return;
    }

    onDecision({
      isApproved: decision === 'approve',
      comment: comment.trim() || undefined
    });
  };

  return (
    <div className="modal-overlay">
      <div className="modal-content approval-decision-modal">
        <h2>Godkjenn eller avvis prosess</h2>
        
        <div className="process-summary">
          <h3>{request.prosessTitle}</h3>
          <p><strong>Forespurt av:</strong> {request.requestedByUserName}</p>
          <p><strong>Dato:</strong> {new Date(request.requestedAt).toLocaleString('no-NO')}</p>
          {request.requestComment && (
            <div className="request-comment">
              <strong>Kommentar fra forespørsel:</strong>
              <p>{request.requestComment}</p>
            </div>
          )}
        </div>

        <form onSubmit={handleSubmit}>
          <div className="decision-options">
            <label className="decision-option">
              <input
                type="radio"
                name="decision"
                value="approve"
                checked={decision === 'approve'}
                onChange={(e) => setDecision(e.target.value as 'approve')}
                disabled={loading}
              />
              <span className="approve-option">✅ Godkjenn prosess</span>
            </label>
            
            <label className="decision-option">
              <input
                type="radio"
                name="decision"
                value="reject"
                checked={decision === 'reject'}
                onChange={(e) => setDecision(e.target.value as 'reject')}
                disabled={loading}
              />
              <span className="reject-option">❌ Avvis prosess</span>
            </label>
          </div>

          <div className="comment-section">
            <label htmlFor="decision-comment">
              {decision === 'reject' ? 'Årsak til avvisning (påkrevd):' : 'Kommentar (valgfri):'}
            </label>
            <textarea
              id="decision-comment"
              value={comment}
              onChange={(e) => setComment(e.target.value)}
              placeholder={decision === 'reject' 
                ? 'Forklar hvorfor prosessen avvises og hva som må endres...'
                : 'Legg til kommentarer eller tilbakemeldinger...'
              }
              rows={4}
              disabled={loading}
              required={decision === 'reject'}
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
              className={`submit-button ${decision === 'approve' ? 'approve' : 'reject'}`}
              disabled={!decision || loading}
            >
              {loading ? (
                <>
                  <span className="spinner-small"></span>
                  Behandler...
                </>
              ) : (
                decision === 'approve' ? '✅ Godkjenn' : '❌ Avvis'
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};