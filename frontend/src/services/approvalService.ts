import { 
  ProsessApprovalRequest, 
  ApprovalQueue, 
  MyApprovalRequests,
  SubmitForApprovalRequest,
  ApprovalDecisionRequest,
  AddApprovalCommentRequest,
  ProsessApprovalComment,
  ProsessApprovalHistory,
  ApprovalQueueStatistics,
  ApprovalNotification
} from '../types/approval.ts';
import { authService } from './authService.ts';

const API_BASE_URL = 'http://localhost:5001/api';

class ApprovalService {
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
      throw new Error(errorData.message || 'Request failed');
    }

    return response.json();
  }

  // Submission and withdrawal
  async submitForApproval(prosessId: number, request: SubmitForApprovalRequest): Promise<ProsessApprovalRequest> {
    return this.makeRequest<ProsessApprovalRequest>(`/approval/submit/${prosessId}`, {
      method: 'POST',
      body: JSON.stringify(request),
    });
  }

  async withdrawApprovalRequest(prosessId: number): Promise<void> {
    await this.makeRequest<void>(`/approval/withdraw/${prosessId}`, {
      method: 'POST',
    });
  }

  // Approval decisions
  async approveProcess(approvalRequestId: number, request: ApprovalDecisionRequest): Promise<ProsessApprovalRequest> {
    return this.makeRequest<ProsessApprovalRequest>(`/approval/approve/${approvalRequestId}`, {
      method: 'POST',
      body: JSON.stringify(request),
    });
  }

  async rejectProcess(approvalRequestId: number, request: ApprovalDecisionRequest): Promise<ProsessApprovalRequest> {
    return this.makeRequest<ProsessApprovalRequest>(`/approval/reject/${approvalRequestId}`, {
      method: 'POST',
      body: JSON.stringify(request),
    });
  }

  // Comments
  async addComment(approvalRequestId: number, request: AddApprovalCommentRequest): Promise<ProsessApprovalComment> {
    return this.makeRequest<ProsessApprovalComment>(`/approval/comment/${approvalRequestId}`, {
      method: 'POST',
      body: JSON.stringify(request),
    });
  }

  async getComments(approvalRequestId: number): Promise<ProsessApprovalComment[]> {
    return this.makeRequest<ProsessApprovalComment[]>(`/approval/request/${approvalRequestId}/comments`);
  }

  // Queues and listings
  async getApprovalQueue(): Promise<ApprovalQueue> {
    return this.makeRequest<ApprovalQueue>('/approval/queue');
  }

  async getMyApprovalRequests(): Promise<MyApprovalRequests> {
    return this.makeRequest<MyApprovalRequests>('/approval/my-requests');
  }

  async getApprovalRequest(approvalRequestId: number): Promise<ProsessApprovalRequest> {
    return this.makeRequest<ProsessApprovalRequest>(`/approval/request/${approvalRequestId}`);
  }

  async getCurrentApprovalRequestForProcess(prosessId: number): Promise<ProsessApprovalRequest | null> {
    try {
      return await this.makeRequest<ProsessApprovalRequest>(`/approval/process/${prosessId}/current`);
    } catch (error: any) {
      if (error.message.includes('ikke funnet') || error.message.includes('not found')) {
        return null;
      }
      throw error;
    }
  }

  // History and audit
  async getApprovalHistory(prosessId: number): Promise<ProsessApprovalHistory[]> {
    return this.makeRequest<ProsessApprovalHistory[]>(`/approval/process/${prosessId}/history`);
  }

  // Permissions and validation
  async canUserApproveProcess(prosessId: number): Promise<boolean> {
    return this.makeRequest<boolean>(`/approval/can-approve/${prosessId}`);
  }

  async canUserSubmitForApproval(prosessId: number): Promise<boolean> {
    return this.makeRequest<boolean>(`/approval/can-submit/${prosessId}`);
  }

  // Statistics
  async getApprovalStatistics(): Promise<ApprovalQueueStatistics> {
    const queue = await this.getApprovalQueue();
    return queue.statistics;
  }

  // Notifications
  async getApprovalNotifications(): Promise<ApprovalNotification[]> {
    return this.makeRequest<ApprovalNotification[]>('/approval/notifications');
  }

  async markNotificationsAsRead(notificationIds: number[]): Promise<void> {
    await this.makeRequest<void>('/approval/notifications/mark-read', {
      method: 'POST',
      body: JSON.stringify({ notificationIds }),
    });
  }

  // Helper methods
  getApprovalStatusText(status: number): string {
    switch (status) {
      case 0: return 'Venter';
      case 1: return 'Under behandling';
      case 2: return 'Godkjent';
      case 3: return 'Avvist';
      case 4: return 'Trukket tilbake';
      default: return 'Ukjent';
    }
  }

  getApprovalStatusColor(status: number): string {
    switch (status) {
      case 0: return '#fbbf24'; // yellow
      case 1: return '#3b82f6'; // blue
      case 2: return '#10b981'; // green
      case 3: return '#ef4444'; // red
      case 4: return '#6b7280'; // gray
      default: return '#6b7280'; // gray
    }
  }

  getCommentTypeText(type: number): string {
    switch (type) {
      case 0: return 'Generell';
      case 1: return 'Spørsmål';
      case 2: return 'Forslag';
      case 3: return 'Problem';
      case 4: return 'Godkjenning';
      case 5: return 'Avvisning';
      default: return 'Generell';
    }
  }
}

export const approvalService = new ApprovalService();