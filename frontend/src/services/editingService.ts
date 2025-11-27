import {
  StartEditSessionRequest,
  StartEditSessionResponse,
  EditProsessRequest,
  SaveDraftRequest,
  ProsessEditSession,
  ProsessDiff,
  ProsessVersionCompareRequest,
  EditingStatistics,
  EditConflict,
  ResolveConflictRequest,
  ProsessLock
} from '../types/editing';
import { ProsessDetail } from '../types/prosess';
import { authService } from './authService.ts';

const API_BASE_URL = (process.env.REACT_APP_API_URL || 'http://localhost:5001') + '/api';

class EditingService {
  private autoSaveInterval: number | null = null;
  private readonly autoSaveIntervalMs = 30000; // 30 seconds

  private async makeRequest<T>(url: string, options: RequestInit = {}): Promise<T> {
    const token = authService.getToken();
    
    const response = await fetch(`${API_BASE_URL}${url}`, {
      ...options,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': token ? `Bearer ${token}` : '',
        ...options.headers,
      },
    });

    if (response.status === 401) {
      authService.logout();
      throw new Error('Unauthorized');
    }

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({ message: 'Request failed' }));
      const error = new Error(errorData.message || 'Request failed');
      (error as any).status = response.status;
      throw error;
    }

    return response.json();
  }

  // Edit session management
  async startEditSession(prosessId: number, request: StartEditSessionRequest): Promise<StartEditSessionResponse> {
    return this.makeRequest<StartEditSessionResponse>(`/editing/start/${prosessId}`, {
      method: 'POST',
      body: JSON.stringify(request),
    });
  }

  async endEditSession(sessionId: string, completionComment?: string): Promise<void> {
    await this.makeRequest<void>(`/editing/end/${sessionId}`, {
      method: 'POST',
      body: JSON.stringify(completionComment),
    });
  }

  async getActiveEditSessions(prosessId: number): Promise<ProsessEditSession[]> {
    return this.makeRequest<ProsessEditSession[]>(`/editing/sessions/${prosessId}`);
  }

  // Draft functionality
  async saveDraft(sessionId: string, request: SaveDraftRequest): Promise<ProsessDetail> {
    return this.makeRequest<ProsessDetail>(`/editing/draft/${sessionId}`, {
      method: 'POST',
      body: JSON.stringify(request),
    });
  }

  async getDraft(sessionId: string): Promise<ProsessDetail | null> {
    try {
      return await this.makeRequest<ProsessDetail>(`/editing/draft/${sessionId}`);
    } catch (error: any) {
      // Handle 404 responses (no draft found) as expected behavior
      if (error.status === 404 || 
          error.message.includes('ikke funnet') || 
          error.message.includes('not found')) {
        return null;
      }
      throw error;
    }
  }

  async autoSave(sessionId: string, request: SaveDraftRequest): Promise<boolean> {
    return this.makeRequest<boolean>(`/editing/autosave/${sessionId}`, {
      method: 'POST',
      body: JSON.stringify(request),
    });
  }

  // Complete editing
  async completeEdit(sessionId: string, request: EditProsessRequest): Promise<ProsessDetail> {
    return this.makeRequest<ProsessDetail>(`/editing/complete/${sessionId}`, {
      method: 'POST',
      body: JSON.stringify(request),
    });
  }

  // Version comparison
  async compareVersions(request: ProsessVersionCompareRequest): Promise<ProsessDiff> {
    return this.makeRequest<ProsessDiff>('/editing/compare', {
      method: 'POST',
      body: JSON.stringify(request),
    });
  }

  async compareWithDraft(sessionId: string, versionId: number): Promise<ProsessDiff> {
    return this.makeRequest<ProsessDiff>(`/editing/compare-draft/${sessionId}/${versionId}`, {
      method: 'POST',
    });
  }

  // Permissions
  async canUserEditProcess(prosessId: number): Promise<boolean> {
    return this.makeRequest<boolean>(`/editing/can-edit/${prosessId}`);
  }

  // Statistics
  async getEditingStatistics(): Promise<EditingStatistics> {
    return this.makeRequest<EditingStatistics>('/editing/statistics');
  }

  // Auto-save management
  startAutoSave(sessionId: string, getDraftData: () => SaveDraftRequest): void {
    if (this.autoSaveInterval) {
      this.stopAutoSave();
    }

    this.autoSaveInterval = window.setInterval(async () => {
      try {
        const draftData = getDraftData();
        await this.autoSave(sessionId, draftData);
      } catch (error) {
        console.error('Auto-save failed:', error);
      }
    }, this.autoSaveIntervalMs);
  }

  stopAutoSave(): void {
    if (this.autoSaveInterval) {
      clearInterval(this.autoSaveInterval);
      this.autoSaveInterval = null;
    }
  }

  // Helper methods
  getChangeTypeText(changeType: number): string {
    switch (changeType) {
      case 0: return 'Lagt til';
      case 1: return 'Endret';
      case 2: return 'Slettet';
      case 3: return 'Flyttet';
      case 4: return 'Uendret';
      default: return 'Ukjent';
    }
  }

  getChangeTypeColor(changeType: number): string {
    switch (changeType) {
      case 0: return '#10b981'; // green - added
      case 1: return '#3b82f6'; // blue - modified
      case 2: return '#ef4444'; // red - deleted
      case 3: return '#f59e0b'; // yellow - moved
      case 4: return '#6b7280'; // gray - no change
      default: return '#6b7280';
    }
  }

  getVersionChangeTypeText(type: number): string {
    switch (type) {
      case 0: return 'Patch (feilrettinger)';
      case 1: return 'Minor (nye funksjoner)';
      case 2: return 'Major (store endringer)';
      default: return 'Minor';
    }
  }

  getEditStatusText(status: number): string {
    switch (status) {
      case 0: return 'Aktiv';
      case 1: return 'Inaktiv';
      case 2: return 'Fullført';
      case 3: return 'Avbrutt';
      case 4: return 'Konflikt oppdaget';
      case 5: return 'Utløpt';
      default: return 'Ukjent';
    }
  }

  formatTimeAgo(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMinutes = Math.floor(diffMs / (1000 * 60));

    if (diffMinutes < 1) return 'Akkurat nå';
    if (diffMinutes < 60) return `${diffMinutes} min siden`;
    
    const diffHours = Math.floor(diffMinutes / 60);
    if (diffHours < 24) return `${diffHours} timer siden`;
    
    const diffDays = Math.floor(diffHours / 24);
    return `${diffDays} dager siden`;
  }

  // Local storage helpers for draft data
  saveDraftToLocalStorage(sessionId: string, draftData: SaveDraftRequest): void {
    const key = `draft_${sessionId}`;
    const dataWithTimestamp = {
      ...draftData,
      lastSaved: new Date().toISOString(),
    };
    localStorage.setItem(key, JSON.stringify(dataWithTimestamp));
  }

  getDraftFromLocalStorage(sessionId: string): (SaveDraftRequest & { lastSaved: string }) | null {
    const key = `draft_${sessionId}`;
    const data = localStorage.getItem(key);
    if (!data) return null;
    
    try {
      return JSON.parse(data);
    } catch {
      return null;
    }
  }

  clearDraftFromLocalStorage(sessionId: string): void {
    const key = `draft_${sessionId}`;
    localStorage.removeItem(key);
  }
}

export const editingService = new EditingService();