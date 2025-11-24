import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { ProsessDetail } from '../../types/prosess';
import { 
  StartEditSessionResponse, 
  EditProsessRequest, 
  SaveDraftRequest,
  VersionChangeType,
  ProsessEditSession,
  AutoSaveData
} from '../../types/editing.ts';
import { editingService } from '../../services/editingService.ts';
import { approvalService } from '../../services/approvalService.ts';
import { ProsessEditForm } from './ProsessEditForm.tsx';
import { EditSessionInfo } from './EditSessionInfo.tsx';
import { AutoSaveIndicator } from './AutoSaveIndicator.tsx';
import { VersionChangeModal } from './VersionChangeModal.tsx';
import './prosess-editor.css';

interface ProsessEditorProps {
  onSaveSuccess?: () => void;
  onCancel?: () => void;
}

export const ProsessEditor: React.FC<ProsessEditorProps> = ({
  onSaveSuccess,
  onCancel
}) => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const prosessId = parseInt(id || '0');
  const [editSession, setEditSession] = useState<StartEditSessionResponse | null>(null);
  const [currentData, setCurrentData] = useState<ProsessDetail | null>(null);
  const [hasUnsavedChanges, setHasUnsavedChanges] = useState(false);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showVersionModal, setShowVersionModal] = useState(false);
  const [pendingSave, setPendingSave] = useState<EditProsessRequest | null>(null);
  
  // Auto-save state
  const [autoSaveData, setAutoSaveData] = useState<AutoSaveData>({
    sessionId: '',
    lastSaved: '',
    hasUnsavedChanges: false,
    autoSaveEnabled: true
  });

  useEffect(() => {
    startEditingSession();
    return () => {
      // Cleanup on unmount
      if (editSession?.sessionId) {
        editingService.stopAutoSave();
        editingService.endEditSession(editSession.sessionId);
      }
    };
  }, [prosessId]);

  const startEditingSession = async () => {
    try {
      setLoading(true);
      setError(null);

      // Check if user can edit
      const canEdit = await editingService.canUserEditProcess(prosessId);
      if (!canEdit) {
        setError('Du har ikke tilgang til å redigere denne prosessen');
        return;
      }

      // Start edit session
      const session = await editingService.startEditSession(prosessId, {
        comment: 'Startet redigeringssesjon'
      });

      setEditSession(session);
      setCurrentData(session.prosess);
      
      // Set up auto-save
      setAutoSaveData({
        sessionId: session.sessionId,
        lastSaved: new Date().toISOString(),
        hasUnsavedChanges: false,
        autoSaveEnabled: true
      });

      // Check for existing draft
      const existingDraft = await editingService.getDraft(session.sessionId);
      if (existingDraft) {
        setCurrentData(existingDraft);
        setHasUnsavedChanges(true);
      }

      // Start auto-save
      editingService.startAutoSave(session.sessionId, () => createDraftRequest());

    } catch (err: any) {
      console.error('Error starting edit session:', err);
      setError(err.message || 'Feil ved start av redigeringssesjon');
    } finally {
      setLoading(false);
    }
  };

  const createDraftRequest = useCallback((): SaveDraftRequest => {
    if (!currentData) {
      throw new Error('No current data for auto-save');
    }

    return {
      title: currentData.title,
      description: currentData.description,
      category: currentData.category,
      tags: currentData.tags.map(t => t.name),
      steps: currentData.steps.map(step => ({
        title: step.title,
        description: step.description,
        type: step.type,
        responsibleRole: step.responsibleRole,
        estimatedDurationMinutes: step.estimatedDurationMinutes,
        orderIndex: step.orderIndex,
        isOptional: step.isOptional,
        detailedInstructions: step.detailedInstructions
      }))
    };
  }, [currentData]);

  const handleFormChange = (updatedProsess: ProsessDetail) => {
    setCurrentData(updatedProsess);
    setHasUnsavedChanges(true);
    
    // Save to local storage as backup
    if (editSession?.sessionId) {
      const draftRequest = {
        title: updatedProsess.title,
        description: updatedProsess.description,
        category: updatedProsess.category,
        tags: updatedProsess.tags.map(t => t.name),
        steps: updatedProsess.steps.map(step => ({
          title: step.title,
          description: step.description,
          type: step.type,
          responsibleRole: step.responsibleRole,
          estimatedDurationMinutes: step.estimatedDurationMinutes,
          orderIndex: step.orderIndex,
          isOptional: step.isOptional,
          detailedInstructions: step.detailedInstructions
        }))
      };
      editingService.saveDraftToLocalStorage(editSession.sessionId, draftRequest);
    }
  };

  const handleSaveDraft = async () => {
    if (!editSession || !currentData) return;

    try {
      setSaving(true);
      const draftRequest = createDraftRequest();
      const savedDraft = await editingService.saveDraft(editSession.sessionId, draftRequest);
      
      setCurrentData(savedDraft);
      setHasUnsavedChanges(false);
      setAutoSaveData(prev => ({
        ...prev,
        lastSaved: new Date().toISOString(),
        hasUnsavedChanges: false
      }));
      
    } catch (err: any) {
      console.error('Error saving draft:', err);
      setError(err.message || 'Feil ved lagring av utkast');
    } finally {
      setSaving(false);
    }
  };

  const handleSaveAndComplete = (changeComment?: string, versionChangeType: VersionChangeType = VersionChangeType.Minor) => {
    if (!currentData) return;

    const editRequest: EditProsessRequest = {
      title: currentData.title,
      description: currentData.description,
      category: currentData.category,
      tags: currentData.tags.map(t => t.name),
      steps: currentData.steps.map(step => ({
        title: step.title,
        description: step.description,
        type: step.type,
        responsibleRole: step.responsibleRole,
        estimatedDurationMinutes: step.estimatedDurationMinutes,
        orderIndex: step.orderIndex,
        isOptional: step.isOptional,
        detailedInstructions: step.detailedInstructions
      })),
      saveAsDraft: false,
      changeComment,
      versionChangeType
    };

    setPendingSave(editRequest);
    setShowVersionModal(true);
  };

  const confirmSave = async () => {
    if (!editSession || !pendingSave) return;

    try {
      setSaving(true);
      const savedProsess = await editingService.completeEdit(editSession.sessionId, pendingSave);
      
      // Cleanup
      editingService.stopAutoSave();
      editingService.clearDraftFromLocalStorage(editSession.sessionId);
      
      setShowVersionModal(false);
      setPendingSave(null);
      
      if (onSaveSuccess) {
        onSaveSuccess();
      } else {
        navigate(`/prosess/${savedProsess.id}`);
      }
      
    } catch (err: any) {
      console.error('Error saving prosess:', err);
      setError(err.message || 'Feil ved lagring av prosess');
    } finally {
      setSaving(false);
    }
  };

  const handleSubmitForApproval = async () => {
    if (!currentData) return;

    try {
      setSaving(true);
      
      // First save any pending changes as a new version
      const editRequest: EditProsessRequest = {
        title: currentData.title,
        description: currentData.description,
        category: currentData.category,
        tags: currentData.tags.map(t => t.name),
        steps: currentData.steps.map(step => ({
          title: step.title,
          description: step.description,
          type: step.type,
          responsibleRole: step.responsibleRole,
          estimatedDurationMinutes: step.estimatedDurationMinutes,
          orderIndex: step.orderIndex,
          isOptional: step.isOptional,
          detailedInstructions: step.detailedInstructions
        })),
        saveAsDraft: false,
        changeComment: 'Klargjort for godkjenning',
        versionChangeType: VersionChangeType.Minor
      };

      if (editSession && hasUnsavedChanges) {
        await editingService.completeEdit(editSession.sessionId, editRequest);
      }

      // Then submit for approval
      await approvalService.submitForApproval(prosessId, {
        requestComment: 'Prosess er klar for godkjenning'
      });

      // Cleanup
      if (editSession?.sessionId) {
        editingService.stopAutoSave();
        editingService.clearDraftFromLocalStorage(editSession.sessionId);
      }

      alert('Prosessen er sendt til godkjenning!');
      navigate(`/prosess/${prosessId}`);
      
    } catch (err: any) {
      console.error('Error submitting for approval:', err);
      setError(err.message || 'Feil ved sending til godkjenning');
    } finally {
      setSaving(false);
    }
  };

  const handleCancel = async () => {
    if (hasUnsavedChanges) {
      const confirmCancel = window.confirm(
        'Du har ulagrede endringer. Er du sikker på at du vil avbryte redigeringen?'
      );
      if (!confirmCancel) return;
    }

    // Cleanup
    if (editSession?.sessionId) {
      editingService.stopAutoSave();
      editingService.clearDraftFromLocalStorage(editSession.sessionId);
      await editingService.endEditSession(editSession.sessionId, 'Avbrutt av bruker');
    }

    if (onCancel) {
      onCancel();
    } else {
      navigate(-1);
    }
  };

  if (loading) {
    return (
      <div className="prosess-editor-loading">
        <div className="spinner"></div>
        <p>Starter redigeringssesjon...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="prosess-editor-error">
        <h3>Feil ved redigering</h3>
        <p>{error}</p>
        <button onClick={() => navigate(-1)} className="back-button">
          Gå tilbake
        </button>
      </div>
    );
  }

  if (!editSession || !currentData) {
    return null;
  }

  return (
    <div className="prosess-editor">
      <div className="prosess-editor-header">
        <h1>Rediger prosess: {currentData.title}</h1>
        <div className="editor-controls">
          <AutoSaveIndicator data={autoSaveData} />
          <EditSessionInfo 
            session={editSession.editSession} 
            otherSessions={editSession.activeSessions}
          />
        </div>
      </div>

      <div className="prosess-editor-content">
        <ProsessEditForm
          prosess={currentData}
          onChange={handleFormChange}
          disabled={saving}
        />
      </div>

      <div className="prosess-editor-actions">
        <button
          onClick={handleSaveDraft}
          disabled={saving || !hasUnsavedChanges}
          className="save-draft-button"
        >
          {saving ? 'Lagrer...' : '💾 Lagre utkast'}
        </button>

        <button
          onClick={() => handleSaveAndComplete()}
          disabled={saving}
          className="save-complete-button"
        >
          {saving ? 'Lagrer...' : '✅ Lagre og fullfør'}
        </button>

        {/* Show submit for approval button only for Draft status */}
        {currentData.status === 0 && (
          <button
            onClick={handleSubmitForApproval}
            disabled={saving}
            className="submit-approval-button"
          >
            {saving ? 'Sender...' : '📋 Send til godkjenning'}
          </button>
        )}

        <button
          onClick={handleCancel}
          disabled={saving}
          className="cancel-button"
        >
          ❌ Avbryt
        </button>
      </div>

      {showVersionModal && pendingSave && (
        <VersionChangeModal
          currentVersion={currentData.versionHistory?.[0]?.versionNumber || '1.0.0'}
          onConfirm={confirmSave}
          onCancel={() => {
            setShowVersionModal(false);
            setPendingSave(null);
          }}
          loading={saving}
        />
      )}
    </div>
  );
};