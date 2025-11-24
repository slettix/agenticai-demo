import React, { useState, useEffect } from 'react';
import { AutoSaveData } from '../../types/editing.ts';

interface AutoSaveIndicatorProps {
  data: AutoSaveData;
}

export const AutoSaveIndicator: React.FC<AutoSaveIndicatorProps> = ({ data }) => {
  const [timeDisplay, setTimeDisplay] = useState('');

  useEffect(() => {
    const updateTimeDisplay = () => {
      if (data.lastSaved) {
        const lastSaved = new Date(data.lastSaved);
        const now = new Date();
        const diffMs = now.getTime() - lastSaved.getTime();
        const diffMinutes = Math.floor(diffMs / (1000 * 60));
        const diffSeconds = Math.floor(diffMs / 1000);

        if (diffSeconds < 60) {
          setTimeDisplay(`${diffSeconds}s siden`);
        } else if (diffMinutes < 60) {
          setTimeDisplay(`${diffMinutes}m siden`);
        } else {
          setTimeDisplay(lastSaved.toLocaleTimeString('no-NO', { 
            hour: '2-digit', 
            minute: '2-digit' 
          }));
        }
      }
    };

    updateTimeDisplay();
    const interval = setInterval(updateTimeDisplay, 1000);

    return () => clearInterval(interval);
  }, [data.lastSaved]);

  const getStatusIcon = () => {
    if (data.hasUnsavedChanges) {
      return '●'; // Filled circle for unsaved changes
    } else {
      return '○'; // Empty circle for saved
    }
  };

  const getStatusText = () => {
    if (data.hasUnsavedChanges) {
      return 'Ulagrede endringer';
    } else {
      return 'Alle endringer lagret';
    }
  };

  const getStatusClass = () => {
    if (data.hasUnsavedChanges) {
      return 'unsaved';
    } else {
      return 'saved';
    }
  };

  return (
    <div className={`autosave-indicator ${getStatusClass()}`}>
      <div className="autosave-status">
        <span className="status-icon">{getStatusIcon()}</span>
        <span className="status-text">{getStatusText()}</span>
      </div>
      
      {data.lastSaved && (
        <div className="last-saved">
          <span>Sist lagret: {timeDisplay}</span>
        </div>
      )}
      
      {data.autoSaveEnabled && (
        <div className="autosave-info">
          <span className="autosave-badge">Auto-lagring på</span>
        </div>
      )}
      
      {data.nextAutoSave && (
        <div className="next-autosave">
          <span>Neste auto-lagring: {new Date(data.nextAutoSave).toLocaleTimeString('no-NO')}</span>
        </div>
      )}
    </div>
  );
};