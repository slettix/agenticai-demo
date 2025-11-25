import { 
  DeleteProsessRequest, 
  RestoreProsessRequest, 
  DeletedProsessDto, 
  DeletionHistoryDto, 
  BulkDeleteRequest, 
  BulkDeleteResult,
  PagedResult 
} from '../types/deletion.ts';

const API_BASE_URL = 'http://localhost:5001/api';

class DeletionService {
  private getAuthHeaders() {
    const token = localStorage.getItem('prosessportal_token');
    console.log('DeletionService token:', token ? `${token.substring(0, 50)}...` : 'null');
    return {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    };
  }

  async softDeleteProcess(prosessId: number, request: DeleteProsessRequest): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/deletion/${prosessId}/soft-delete`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(request)
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`Failed to delete process: ${errorText}`);
    }
  }

  async hardDeleteProcess(prosessId: number, request: DeleteProsessRequest): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/deletion/${prosessId}/hard-delete`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(request)
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`Failed to permanently delete process: ${errorText}`);
    }
  }

  async restoreProcess(prosessId: number, request: RestoreProsessRequest): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/deletion/${prosessId}/restore`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(request)
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`Failed to restore process: ${errorText}`);
    }
  }

  async getDeletedProcesses(page: number = 1, pageSize: number = 20): Promise<PagedResult<DeletedProsessDto>> {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString()
    });

    const response = await fetch(`${API_BASE_URL}/deletion/deleted?${params}`, {
      method: 'GET',
      headers: this.getAuthHeaders()
    });

    if (!response.ok) {
      throw new Error('Failed to fetch deleted processes');
    }

    return await response.json();
  }

  async bulkDeleteProcesses(request: BulkDeleteRequest): Promise<BulkDeleteResult> {
    const response = await fetch(`${API_BASE_URL}/deletion/bulk-delete`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(request)
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`Failed to bulk delete processes: ${errorText}`);
    }

    return await response.json();
  }

  async getDeletionHistory(prosessId: number): Promise<DeletionHistoryDto[]> {
    const response = await fetch(`${API_BASE_URL}/deletion/${prosessId}/deletion-history`, {
      method: 'GET',
      headers: this.getAuthHeaders()
    });

    if (!response.ok) {
      throw new Error('Failed to fetch deletion history');
    }

    return await response.json();
  }

  async hasActiveInstances(prosessId: number): Promise<boolean> {
    const response = await fetch(`${API_BASE_URL}/deletion/${prosessId}/has-active-instances`, {
      method: 'GET',
      headers: this.getAuthHeaders()
    });

    if (!response.ok) {
      throw new Error('Failed to check active instances');
    }

    return await response.json();
  }

  async canUserDelete(prosessId: number): Promise<boolean> {
    const response = await fetch(`${API_BASE_URL}/deletion/${prosessId}/can-delete`, {
      method: 'GET',
      headers: this.getAuthHeaders()
    });

    if (!response.ok) {
      throw new Error('Failed to check delete permissions');
    }

    return await response.json();
  }
}

export const deletionService = new DeletionService();