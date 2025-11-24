import React from 'react';
import { ApprovalQueueStatistics } from '../../types/approval.ts';

interface ApprovalStatisticsProps {
  statistics: ApprovalQueueStatistics;
}

export const ApprovalStatistics: React.FC<ApprovalStatisticsProps> = ({ statistics }) => {
  const formatTimeHours = (hours: number) => {
    if (hours < 1) {
      return `${Math.round(hours * 60)} min`;
    } else if (hours < 24) {
      return `${hours.toFixed(1)} timer`;
    } else {
      const days = Math.floor(hours / 24);
      const remainingHours = hours % 24;
      return `${days}d ${remainingHours.toFixed(1)}t`;
    }
  };

  return (
    <div className="approval-statistics">
      <h2>Statistikk</h2>
      
      <div className="stats-grid">
        <div className="stat-card">
          <div className="stat-number">{statistics.totalPendingApprovals}</div>
          <div className="stat-label">Venter på godkjenning</div>
          <div className="stat-icon">⏳</div>
        </div>

        <div className="stat-card">
          <div className="stat-number">{statistics.totalInProgressApprovals}</div>
          <div className="stat-label">Under behandling</div>
          <div className="stat-icon">🔄</div>
        </div>

        <div className="stat-card">
          <div className="stat-number">{statistics.totalCompletedThisMonth}</div>
          <div className="stat-label">Fullført denne måneden</div>
          <div className="stat-icon">✅</div>
        </div>

        <div className="stat-card">
          <div className="stat-number">
            {statistics.averageApprovalTimeHours > 0 
              ? formatTimeHours(statistics.averageApprovalTimeHours)
              : 'N/A'
            }
          </div>
          <div className="stat-label">Gjennomsnittlig behandlingstid</div>
          <div className="stat-icon">⏰</div>
        </div>

        <div className="stat-card personal">
          <div className="stat-number">{statistics.myPendingApprovals}</div>
          <div className="stat-label">Mine ventende godkjenninger</div>
          <div className="stat-icon">👤</div>
        </div>

        <div className="stat-card personal">
          <div className="stat-number">{statistics.myCompletedApprovals}</div>
          <div className="stat-label">Mine fullførte godkjenninger</div>
          <div className="stat-icon">🏆</div>
        </div>
      </div>
    </div>
  );
};