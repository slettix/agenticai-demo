import React, { useState } from 'react';
import { VersionChangeType } from '../../types/editing.ts';

interface VersionChangeModalProps {
  currentVersion: string;
  onConfirm: () => void;
  onCancel: () => void;
  loading?: boolean;
}

export const VersionChangeModal: React.FC<VersionChangeModalProps> = ({
  currentVersion,
  onConfirm,
  onCancel,
  loading = false
}) => {
  const [versionType, setVersionType] = useState<VersionChangeType>(VersionChangeType.Minor);
  const [changeComment, setChangeComment] = useState('');

  const getNextVersion = (current: string, type: VersionChangeType): string => {
    const parts = current.split('.').map(Number);
    if (parts.length !== 3) return '1.0.0';

    switch (type) {
      case VersionChangeType.Major:
        return `${parts[0] + 1}.0.0`;
      case VersionChangeType.Minor:
        return `${parts[0]}.${parts[1] + 1}.0`;
      case VersionChangeType.Patch:
        return `${parts[0]}.${parts[1]}.${parts[2] + 1}`;
      default:
        return current;
    }
  };

  const nextVersion = getNextVersion(currentVersion, versionType);

  const handleConfirm = () => {
    onConfirm();
  };

  return (
    <div className="modal-overlay">
      <div className="modal-content version-change-modal">
        <h2>Lagre endringer</h2>
        
        <div className="version-info">
          <div className="current-version">
            <label>Nåværende versjon:</label>
            <span className="version-number">{currentVersion}</span>
          </div>
          <div className="arrow">→</div>
          <div className="new-version">
            <label>Ny versjon:</label>
            <span className="version-number new">{nextVersion}</span>
          </div>
        </div>

        <div className="version-type-selection">
          <h3>Type endring:</h3>
          
          <div className="version-options">
            <label className="version-option">
              <input
                type="radio"
                name="versionType"
                value={VersionChangeType.Patch}
                checked={versionType === VersionChangeType.Patch}
                onChange={(e) => setVersionType(Number(e.target.value) as VersionChangeType)}
                disabled={loading}
              />
              <div className="option-content">
                <strong>Patch ({getNextVersion(currentVersion, VersionChangeType.Patch)})</strong>
                <p>Små feilrettinger og mindre forbedringer</p>
              </div>
            </label>
            
            <label className="version-option">
              <input
                type="radio"
                name="versionType"
                value={VersionChangeType.Minor}
                checked={versionType === VersionChangeType.Minor}
                onChange={(e) => setVersionType(Number(e.target.value) as VersionChangeType)}
                disabled={loading}
              />
              <div className="option-content">
                <strong>Minor ({getNextVersion(currentVersion, VersionChangeType.Minor)})</strong>
                <p>Nye funksjoner og forbedringer som ikke bryter eksisterende bruk</p>
              </div>
            </label>
            
            <label className="version-option">
              <input
                type="radio"
                name="versionType"
                value={VersionChangeType.Major}
                checked={versionType === VersionChangeType.Major}
                onChange={(e) => setVersionType(Number(e.target.value) as VersionChangeType)}
                disabled={loading}
              />
              <div className="option-content">
                <strong>Major ({getNextVersion(currentVersion, VersionChangeType.Major)})</strong>
                <p>Store endringer som kan påvirke eksisterende bruk av prosessen</p>
              </div>
            </label>
          </div>
        </div>

        <div className="change-comment">
          <label htmlFor="change-comment">
            Beskriv endringene (valgfri):
          </label>
          <textarea
            id="change-comment"
            value={changeComment}
            onChange={(e) => setChangeComment(e.target.value)}
            disabled={loading}
            rows={4}
            placeholder="Beskriv hva som er endret i denne versjonen..."
          />
        </div>

        <div className="version-creation-info">
          <h4>Hva skjer når du lagrer?</h4>
          <ul>
            <li>En ny versjon ({nextVersion}) blir opprettet</li>
            <li>Prosessen får status "Utkast" og må godkjennes på nytt</li>
            <li>Den forrige versjonen ({currentVersion}) beholdes som historikk</li>
            <li>Alle endringer blir logget i endringshistorikken</li>
          </ul>
        </div>

        <div className="modal-actions">
          <button
            onClick={onCancel}
            disabled={loading}
            className="cancel-button"
          >
            Avbryt
          </button>
          <button
            onClick={handleConfirm}
            disabled={loading}
            className="confirm-button"
          >
            {loading ? (
              <>
                <span className="spinner-small"></span>
                Lagrer...
              </>
            ) : (
              `✅ Opprett versjon ${nextVersion}`
            )}
          </button>
        </div>
      </div>
    </div>
  );
};