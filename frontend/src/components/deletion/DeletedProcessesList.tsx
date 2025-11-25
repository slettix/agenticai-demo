import React, { useState, useEffect } from 'react';
import { deletionService } from '../../services/deletionService.ts';
import { DeletedProsessDto, PagedResult, ProsessStatus } from '../../types/deletion.ts';
import { RestoreConfirmationModal } from './RestoreConfirmationModal.tsx';
import './deletion.css';

export const DeletedProcessesList: React.FC = () => {
  const [deletedProcesses, setDeletedProcesses] = useState<PagedResult<DeletedProsessDto>>({
    items: [],
    totalCount: 0,
    page: 1,
    pageSize: 20,
    totalPages: 0
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string>('');
  const [currentPage, setCurrentPage] = useState(1);
  const [selectedProcess, setSelectedProcess] = useState<DeletedProsessDto | null>(null);
  const [showRestoreModal, setShowRestoreModal] = useState(false);

  useEffect(() => {
    loadDeletedProcesses();
  }, [currentPage]);

  const loadDeletedProcesses = async () => {
    try {
      setLoading(true);
      const result = await deletionService.getDeletedProcesses(currentPage, 20);
      setDeletedProcesses(result);
      setError('');
    } catch (err: any) {
      setError(err.message || 'Feil ved lasting av slettede prosesser');
    } finally {
      setLoading(false);
    }
  };

  const handleRestore = (prosess: DeletedProsessDto) => {
    setSelectedProcess(prosess);
    setShowRestoreModal(true);
  };

  const handleRestoreConfirm = async (reason: string) => {
    if (!selectedProcess) return;

    try {
      await deletionService.restoreProcess(selectedProcess.id, { reason });
      setShowRestoreModal(false);
      setSelectedProcess(null);
      await loadDeletedProcesses(); // Reload the list
      alert(`Prosessen "${selectedProcess.title}" ble gjenopprettet!`);
    } catch (err: any) {
      alert(`Feil ved gjenoppretting: ${err.message}`);
    }
  };

  const getStatusText = (status: number): string => {
    switch (status) {
      case 0: return 'Utkast';
      case 1: return 'Til godkjenning';
      case 2: return 'Under review';
      case 3: return 'Godkjent';
      case 4: return 'Avvist';
      case 5: return 'Publisert';
      case 6: return 'Utdatert';
      case 7: return 'Arkivert';
      case 8: return 'Slettet';
      default: return 'Ukjent';
    }
  };

  const getStatusColor = (status: number): string => {
    switch (status) {
      case 0: return '#6c757d';  // Draft
      case 1: return '#ffc107';  // PendingApproval
      case 2: return '#17a2b8';  // InReview
      case 3: return '#28a745';  // Approved
      case 4: return '#dc3545';  // Rejected
      case 5: return '#007bff';  // Published
      case 6: return '#fd7e14';  // Deprecated
      case 7: return '#6f42c1';  // Archived
      case 8: return '#343a40';  // Deleted
      default: return '#6c757d';
    }
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

  if (loading && deletedProcesses.items.length === 0) {
    return <div className="loading-spinner">Laster slettede prosesser...</div>;
  }

  return (
    <div className="deleted-processes-list">
      <div className="page-header">
        <h2>🗑️ Slettede prosesser</h2>
        <p>Oversikt over prosesser som er slettet og kan gjenopprettes</p>
      </div>

      {error && (
        <div className="error-message">
          {error}
        </div>
      )}

      {deletedProcesses.items.length === 0 && !loading ? (
        <div className="no-results">
          <h3>Ingen slettede prosesser funnet</h3>
          <p>Det er ingen prosesser som er slettet for øyeblikket.</p>
        </div>
      ) : (
        <>
          <div className="deleted-processes-grid">
            {deletedProcesses.items.map(prosess => (
              <div key={prosess.id} className="deleted-process-card">
                <div className="card-header">
                  <h3 className="process-title">{prosess.title}</h3>
                  <div 
                    className="status-badge"
                    style={{ backgroundColor: getStatusColor(prosess.status) }}
                  >
                    {getStatusText(prosess.status)}
                  </div>
                </div>
                
                <p className="process-description">{prosess.description}</p>
                
                <div className="process-meta">
                  <div className="category">📁 {prosess.category}</div>
                  <div className="deleted-info">
                    🗑️ Slettet {formatDate(prosess.deletedAt)} av {prosess.deletedByUser}
                  </div>
                  {prosess.reason && (
                    <div className="deletion-reason">
                      <strong>Grunn:</strong> {prosess.reason}
                    </div>
                  )}
                </div>
                
                <div className="card-footer">
                  {prosess.canRestore ? (
                    <button
                      onClick={() => handleRestore(prosess)}
                      className="btn-restore"
                      title="Gjenopprett prosess"
                    >
                      ↩️ Gjenopprett
                    </button>
                  ) : (
                    <span className="no-restore-permission">
                      Ingen tilgang til gjenoppretting
                    </span>
                  )}
                </div>
              </div>
            ))}
          </div>

          {loading && (
            <div className="loading-overlay">Laster...</div>
          )}

          {deletedProcesses.totalPages > 1 && (
            <div className="pagination">
              <button 
                onClick={() => setCurrentPage(p => p - 1)}
                disabled={currentPage === 1}
                className="page-btn"
              >
                « Forrige
              </button>
              
              <span className="page-info">
                Side {deletedProcesses.page} av {deletedProcesses.totalPages} 
                ({deletedProcesses.totalCount} slettede prosesser totalt)
              </span>
              
              <button 
                onClick={() => setCurrentPage(p => p + 1)}
                disabled={currentPage === deletedProcesses.totalPages}
                className="page-btn"
              >
                Neste »
              </button>
            </div>
          )}
        </>
      )}

      {showRestoreModal && selectedProcess && (
        <RestoreConfirmationModal
          prosess={selectedProcess}
          isOpen={showRestoreModal}
          onClose={() => {
            setShowRestoreModal(false);
            setSelectedProcess(null);
          }}
          onConfirm={handleRestoreConfirm}
        />
      )}
    </div>
  );
};