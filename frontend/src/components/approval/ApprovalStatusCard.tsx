import React, { useState, useEffect } from 'react';
import { ProsessDetail, ProsessStatus } from '../../types/prosess.ts';
import { ProsessApprovalRequest, SubmitForApprovalRequest } from '../../types/approval.ts';
import { approvalService } from '../../services/approvalService.ts';
import { SubmitForApprovalModal } from './SubmitForApprovalModal.tsx';
import './approval.css';

interface ApprovalStatusCardProps {
  prosess: ProsessDetail;
  onStatusChange: () => void;
}

export const ApprovalStatusCard: React.FC<ApprovalStatusCardProps> = ({
  prosess,
  onStatusChange
}) => {
  const [currentRequest, setCurrentRequest] = useState<ProsessApprovalRequest | null>(null);
  const [canSubmit, setCanSubmit] = useState(false);
  const [canApprove, setCanApprove] = useState(false);
  const [showSubmitModal, setShowSubmitModal] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadApprovalData();
  }, [prosess.id, prosess.status]);

  const loadApprovalData = async () => {
    try {
      setError(null);
      
      // Load current approval request if in approval workflow
      if (prosess.status === ProsessStatus.PendingApproval || 
          prosess.status === ProsessStatus.InReview) {
        const request = await approvalService.getCurrentApprovalRequestForProcess(prosess.id);
        setCurrentRequest(request);
      } else {
        setCurrentRequest(null);
      }

      // Check permissions
      if (prosess.status === ProsessStatus.Draft) {
        const submitPermission = await approvalService.canUserSubmitForApproval(prosess.id);
        setCanSubmit(submitPermission);
      }

      if (prosess.status === ProsessStatus.PendingApproval || 
          prosess.status === ProsessStatus.InReview) {
        const approvePermission = await approvalService.canUserApproveProcess(prosess.id);
        setCanApprove(approvePermission);
      }
    } catch (err: any) {
      console.error('Error loading approval data:', err);
      setError(err.message || 'Feil ved lasting av godkjenningsdata');
    }
  };

  const handleSubmitForApproval = async (request: SubmitForApprovalRequest) => {
    try {
      setLoading(true);
      setError(null);

      await approvalService.submitForApproval(prosess.id, request);
      setShowSubmitModal(false);
      onStatusChange();
    } catch (err: any) {
      console.error('Error submitting for approval:', err);
      setError(err.message || 'Feil ved sending til godkjenning');
    } finally {
      setLoading(false);
    }
  };

  const handleWithdrawRequest = async () => {
    if (!confirm('Er du sikker på at du vil trekke tilbake godkjenningsforespørselen?')) {
      return;
    }

    try {
      setLoading(true);
      setError(null);

      await approvalService.withdrawApprovalRequest(prosess.id);
      onStatusChange();
    } catch (err: any) {
      console.error('Error withdrawing approval request:', err);
      setError(err.message || 'Feil ved tilbaketrekking av forespørsel');
    } finally {
      setLoading(false);
    }
  };

  const getStatusInfo = () => {
    switch (prosess.status) {
      case ProsessStatus.Draft:
        return {
          icon: '📝',
          text: 'Utkast',
          description: 'Prosessen er under utvikling og kan redigeres.',
          color: '#6b7280'
        };
      case ProsessStatus.PendingApproval:
        return {
          icon: '⏳',
          text: 'Venter på godkjenning',
          description: 'Prosessen er sendt til godkjenning og venter på behandling.',
          color: '#fbbf24'
        };
      case ProsessStatus.InReview:
        return {
          icon: '🔄',
          text: 'Under godkjenning',
          description: 'Prosessen er under godkjenning av en godkjenner.',
          color: '#3b82f6'
        };
      case ProsessStatus.Approved:
        return {
          icon: '✅',
          text: 'Godkjent',
          description: 'Prosessen er godkjent og klar for publisering.',
          color: '#10b981'
        };
      case ProsessStatus.Rejected:
        return {
          icon: '❌',
          text: 'Avvist',
          description: 'Prosessen er avvist og må revideres før ny godkjenning.',
          color: '#ef4444'
        };
      case ProsessStatus.Published:
        return {
          icon: '📢',
          text: 'Publisert',
          description: 'Prosessen er publisert og i bruk.',
          color: '#059669'
        };
      default:
        return {
          icon: '❓',
          text: 'Ukjent status',
          description: '',
          color: '#6b7280'
        };
    }
  };

  const statusInfo = getStatusInfo();

  return (
    <>
      <div className="approval-status-card">
        <div className="status-header">
          <div className="status-info">
            <span className="status-icon" style={{ color: statusInfo.color }}>
              {statusInfo.icon}
            </span>
            <div>
              <h3 className="status-text">{statusInfo.text}</h3>
              <p className="status-description">{statusInfo.description}</p>
            </div>
          </div>
        </div>

        {currentRequest && (
          <div className="current-request-info">
            <h4>Godkjenningsforespørsel</h4>
            <p><strong>Forespurt av:</strong> {currentRequest.requestedByUserName}</p>
            <p><strong>Dato:</strong> {new Date(currentRequest.requestedAt).toLocaleString('no-NO')}</p>
            {currentRequest.requestComment && (
              <div className="request-comment">
                <strong>Kommentar:</strong>
                <p>{currentRequest.requestComment}</p>
              </div>
            )}
          </div>
        )}

        {prosess.status === ProsessStatus.Rejected && currentRequest?.rejectionReason && (
          <div className="rejection-info">
            <h4>Årsak til avvisning</h4>
            <p>{currentRequest.rejectionReason}</p>
            {currentRequest.approvedByUserName && (
              <p className="rejected-by">
                Avvist av: <strong>{currentRequest.approvedByUserName}</strong>
                {currentRequest.rejectedAt && (
                  <span> - {new Date(currentRequest.rejectedAt).toLocaleString('no-NO')}</span>
                )}
              </p>
            )}
          </div>
        )}

        <div className="approval-actions">
          {prosess.status === ProsessStatus.Draft && canSubmit && (
            <button
              onClick={() => setShowSubmitModal(true)}
              className="submit-approval-button"
              disabled={loading}
            >
              📤 Send til godkjenning
            </button>
          )}

          {(prosess.status === ProsessStatus.PendingApproval || prosess.status === ProsessStatus.InReview) && 
           currentRequest && currentRequest.requestedByUserName && (
            <button
              onClick={handleWithdrawRequest}
              className="withdraw-button"
              disabled={loading}
            >
              ↩️ Trekk tilbake forespørsel
            </button>
          )}

          {(prosess.status === ProsessStatus.PendingApproval || prosess.status === ProsessStatus.InReview) && 
           canApprove && (
            <button
              onClick={() => window.open(`/godkjenning?prosess=${prosess.id}`, '_blank')}
              className="view-approval-button"
            >
              🔍 Se godkjenningsdetaljer
            </button>
          )}

          {prosess.status === ProsessStatus.Rejected && canSubmit && (
            <button
              onClick={() => setShowSubmitModal(true)}
              className="resubmit-button"
              disabled={loading}
            >
              📤 Send til ny godkjenning
            </button>
          )}
        </div>

        {error && (
          <div className="error-message">
            {error}
          </div>
        )}
      </div>

      {showSubmitModal && (
        <SubmitForApprovalModal
          prosess={prosess}
          onSubmit={handleSubmitForApproval}
          onCancel={() => setShowSubmitModal(false)}
          loading={loading}
        />
      )}
    </>
  );
};