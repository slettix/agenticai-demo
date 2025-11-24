import React, { useState } from 'react';
import { ProsessApprovalRequest, ApprovalDecisionRequest } from '../../types/approval.ts';
import { approvalService } from '../../services/approvalService.ts';
import { ApprovalDecisionModal } from './ApprovalDecisionModal.tsx';
import { CommentsSection } from './CommentsSection.tsx';

interface ApprovalRequestCardProps {
  request: ProsessApprovalRequest;
  showActions: boolean;
  onApprovalAction: () => void;
}

export const ApprovalRequestCard: React.FC<ApprovalRequestCardProps> = ({
  request,
  showActions,
  onApprovalAction
}) => {
  const [showDecisionModal, setShowDecisionModal] = useState(false);
  const [showComments, setShowComments] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [canApprove, setCanApprove] = useState<boolean | null>(null);

  React.useEffect(() => {
    checkApprovalPermission();
  }, [request.prosessId]);

  const checkApprovalPermission = async () => {
    try {
      const permission = await approvalService.canUserApproveProcess(request.prosessId);
      setCanApprove(permission);
    } catch (err) {
      console.error('Error checking approval permission:', err);
      setCanApprove(false);
    }
  };

  const handleApprovalDecision = async (decision: ApprovalDecisionRequest) => {
    try {
      setLoading(true);
      setError(null);

      if (decision.isApproved) {
        await approvalService.approveProcess(request.id, decision);
      } else {
        await approvalService.rejectProcess(request.id, decision);
      }

      setShowDecisionModal(false);
      onApprovalAction();
    } catch (err: any) {
      console.error('Error processing approval decision:', err);
      setError(err.message || 'Feil ved behandling av godkjenning');
    } finally {
      setLoading(false);
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString('no-NO');
  };

  const getStatusIcon = (status: number) => {
    switch (status) {
      case 0: return '⏳'; // Pending
      case 1: return '🔄'; // In Progress
      case 2: return '✅'; // Approved
      case 3: return '❌'; // Rejected
      case 4: return '↩️'; // Withdrawn
      default: return '❓';
    }
  };

  return (
    <>
      <div className="approval-request-card">
        <div className="card-header">
          <div className="process-info">
            <h3>{request.prosessTitle}</h3>
            <p className="requested-by">
              Forespurt av: <strong>{request.requestedByUserName}</strong>
            </p>
            <p className="requested-date">
              {formatDate(request.requestedAt)}
            </p>
          </div>
          <div className="status-badge">
            <span className="status-icon">{getStatusIcon(request.status)}</span>
            <span className="status-text">
              {approvalService.getApprovalStatusText(request.status)}
            </span>
          </div>
        </div>

        {request.requestComment && (
          <div className="request-comment">
            <h4>Kommentar fra forespørsel:</h4>
            <p>{request.requestComment}</p>
          </div>
        )}

        {(request.approvalComment || request.rejectionReason) && (
          <div className="decision-comment">
            <h4>{request.approvalComment ? 'Godkjenningskommentar:' : 'Avvisningsgrunn:'}</h4>
            <p>{request.approvalComment || request.rejectionReason}</p>
            {request.approvedByUserName && (
              <p className="approved-by">
                Behandlet av: <strong>{request.approvedByUserName}</strong>
                {(request.approvedAt || request.rejectedAt) && (
                  <span className="decision-date">
                    {' '}- {formatDate(request.approvedAt || request.rejectedAt!)}
                  </span>
                )}
              </p>
            )}
          </div>
        )}

        <div className="card-actions">
          <button 
            onClick={() => setShowComments(!showComments)}
            className="comments-button"
          >
            💬 Kommentarer ({request.comments.length})
          </button>

          {showActions && canApprove && request.status === 0 && ( // Pending
            <>
              <button 
                onClick={() => setShowDecisionModal(true)}
                className="approve-button"
                disabled={loading}
              >
                ✅ Godkjenn/Avvis
              </button>
            </>
          )}

          <button 
            onClick={() => window.open(`/prosess/${request.prosessId}`, '_blank')}
            className="view-process-button"
          >
            👁️ Se prosess
          </button>
        </div>

        {error && (
          <div className="error-message">
            {error}
          </div>
        )}

        {showComments && (
          <CommentsSection 
            approvalRequestId={request.id}
            existingComments={request.comments}
          />
        )}
      </div>

      {showDecisionModal && (
        <ApprovalDecisionModal
          request={request}
          onDecision={handleApprovalDecision}
          onCancel={() => setShowDecisionModal(false)}
          loading={loading}
        />
      )}
    </>
  );
};