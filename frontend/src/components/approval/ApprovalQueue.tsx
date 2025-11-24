import React, { useState, useEffect } from 'react';
import { approvalService } from '../../services/approvalService.ts';
import { ApprovalQueue as ApprovalQueueType, ProsessApprovalRequest } from '../../types/approval.ts';
import { ApprovalRequestCard } from './ApprovalRequestCard.tsx';
import { ApprovalStatistics } from './ApprovalStatistics.tsx';
import './approval.css';

export const ApprovalQueue: React.FC = () => {
  const [queue, setQueue] = useState<ApprovalQueueType | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<'pending' | 'inprogress' | 'completed'>('pending');

  useEffect(() => {
    loadApprovalQueue();
  }, []);

  const loadApprovalQueue = async () => {
    try {
      setLoading(true);
      setError(null);
      const queueData = await approvalService.getApprovalQueue();
      setQueue(queueData);
    } catch (err: any) {
      console.error('Error loading approval queue:', err);
      if (err.message.includes('Unauthorized') || err.message.includes('ikke tilgang')) {
        setError('Du har ikke tilgang til godkjenningskøen. Kontakt administrator for å få godkjennertilgang.');
      } else {
        setError(err.message || 'Feil ved lasting av godkjenningskø');
      }
    } finally {
      setLoading(false);
    }
  };

  const handleApprovalAction = async () => {
    // Refresh queue after approval action
    await loadApprovalQueue();
  };

  if (loading) {
    return (
      <div className="approval-queue">
        <div className="loading-spinner">
          <div className="spinner"></div>
          <p>Laster godkjenningskø...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="approval-queue">
        <div className="error-message">
          <h3>Feil ved lasting av godkjenningskø</h3>
          <p>{error}</p>
          <button onClick={loadApprovalQueue} className="retry-button">
            Prøv igjen
          </button>
        </div>
      </div>
    );
  }

  if (!queue) {
    return null;
  }

  const getCurrentTabRequests = (): ProsessApprovalRequest[] => {
    switch (activeTab) {
      case 'pending':
        return queue.pendingApprovals;
      case 'inprogress':
        return queue.inProgressApprovals;
      case 'completed':
        return queue.recentlyCompletedApprovals;
      default:
        return [];
    }
  };

  return (
    <div className="approval-queue">
      <div className="approval-queue-header">
        <h1>Godkjenningskø</h1>
        <button onClick={loadApprovalQueue} className="refresh-button">
          <span className="refresh-icon">🔄</span>
          Oppdater
        </button>
      </div>

      <ApprovalStatistics statistics={queue.statistics} />

      <div className="approval-tabs">
        <button 
          className={`tab-button ${activeTab === 'pending' ? 'active' : ''}`}
          onClick={() => setActiveTab('pending')}
        >
          Venter på godkjenning ({queue.pendingApprovals.length})
        </button>
        <button 
          className={`tab-button ${activeTab === 'inprogress' ? 'active' : ''}`}
          onClick={() => setActiveTab('inprogress')}
        >
          Under behandling ({queue.inProgressApprovals.length})
        </button>
        <button 
          className={`tab-button ${activeTab === 'completed' ? 'active' : ''}`}
          onClick={() => setActiveTab('completed')}
        >
          Nylig fullført ({queue.recentlyCompletedApprovals.length})
        </button>
      </div>

      <div className="approval-list">
        {getCurrentTabRequests().length === 0 ? (
          <div className="empty-state">
            <p>Ingen godkjenningsforespørsler i denne kategorien.</p>
          </div>
        ) : (
          getCurrentTabRequests().map((request) => (
            <ApprovalRequestCard 
              key={request.id} 
              request={request}
              showActions={activeTab !== 'completed'}
              onApprovalAction={handleApprovalAction}
            />
          ))
        )}
      </div>
    </div>
  );
};