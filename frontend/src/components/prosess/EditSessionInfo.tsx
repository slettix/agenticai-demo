import React, { useState } from 'react';
import { ProsessEditSession } from '../../types/editing.ts';
import { editingService } from '../../services/editingService.ts';

interface EditSessionInfoProps {
  session: ProsessEditSession;
  otherSessions: ProsessEditSession[];
}

export const EditSessionInfo: React.FC<EditSessionInfoProps> = ({
  session,
  otherSessions
}) => {
  const [showDetails, setShowDetails] = useState(false);

  const hasOtherSessions = otherSessions.length > 0;

  return (
    <div className="edit-session-info">
      <div className="session-summary" onClick={() => setShowDetails(!showDetails)}>
        <div className="session-indicator">
          <div className={`session-status ${session.isActive ? 'active' : 'inactive'}`} />
          <span>Redigeringssesjon</span>
        </div>
        
        {hasOtherSessions && (
          <div className="other-sessions-warning">
            ⚠️ {otherSessions.length} andre redigerer
          </div>
        )}
        
        <button type="button" className="toggle-details">
          {showDetails ? '▼' : '▶'}
        </button>
      </div>

      {showDetails && (
        <div className="session-details">
          <div className="current-session">
            <h4>Din redigeringssesjon</h4>
            <div className="session-info">
              <p><strong>Bruker:</strong> {session.userName}</p>
              <p><strong>Startet:</strong> {editingService.formatTimeAgo(session.startedAt)}</p>
              <p><strong>Siste aktivitet:</strong> {editingService.formatTimeAgo(session.lastActivity)}</p>
              <p><strong>Status:</strong> {editingService.getEditStatusText(session.status)}</p>
            </div>
          </div>

          {hasOtherSessions && (
            <div className="other-sessions">
              <h4>Andre aktive sesjoner</h4>
              <div className="warning-message">
                <p>
                  ⚠️ Andre brukere redigerer denne prosessen samtidig. 
                  Vær oppmerksom på mulige konflikter når du lagrer endringene.
                </p>
              </div>
              
              <div className="sessions-list">
                {otherSessions.map((otherSession) => (
                  <div key={otherSession.sessionId} className="session-item">
                    <div className="session-user">
                      <strong>{otherSession.userName}</strong>
                    </div>
                    <div className="session-timing">
                      <span>Startet: {editingService.formatTimeAgo(otherSession.startedAt)}</span>
                      <span>Aktiv: {editingService.formatTimeAgo(otherSession.lastActivity)}</span>
                    </div>
                    <div className={`session-status-badge ${otherSession.isActive ? 'active' : 'inactive'}`}>
                      {editingService.getEditStatusText(otherSession.status)}
                    </div>
                  </div>
                ))}
              </div>
              
              <div className="conflict-prevention-tips">
                <h5>Tips for å unngå konflikter:</h5>
                <ul>
                  <li>Kommuniser med andre som redigerer prosessen</li>
                  <li>Lagre endringer ofte som utkast</li>
                  <li>Fokuser på forskjellige deler av prosessen</li>
                  <li>Vurder å vente til andre er ferdige hvis det er store endringer</li>
                </ul>
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
};