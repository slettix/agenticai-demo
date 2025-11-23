import { 
  ProsessListItem, 
  ProsessDetail, 
  ProsessSearchRequest, 
  PagedResult, 
  CreateProsessRequest,
  UpdateProsessRequest,
  ProsessStatistics,
  ProsessTag
} from '../types/prosess.ts';
import { authService } from './authService.ts';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000/api';

class ProsessService {
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
      const errorText = await response.text();
      throw new Error(errorText || 'Request failed');
    }

    return response.json();
  }

  async searchProsesses(searchRequest: ProsessSearchRequest): Promise<PagedResult<ProsessListItem>> {
    const params = new URLSearchParams();
    
    Object.entries(searchRequest).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        params.append(key, value.toString());
      }
    });

    return this.makeRequest<PagedResult<ProsessListItem>>(`/prosess?${params.toString()}`);
  }

  async getProsess(id: number): Promise<ProsessDetail> {
    return this.makeRequest<ProsessDetail>(`/prosess/${id}`);
  }

  async createProsess(request: CreateProsessRequest): Promise<ProsessDetail> {
    return this.makeRequest<ProsessDetail>('/prosess', {
      method: 'POST',
      body: JSON.stringify(request),
    });
  }

  async updateProsess(id: number, request: UpdateProsessRequest): Promise<ProsessDetail> {
    return this.makeRequest<ProsessDetail>(`/prosess/${id}`, {
      method: 'PUT',
      body: JSON.stringify(request),
    });
  }

  async deleteProsess(id: number): Promise<void> {
    await this.makeRequest<void>(`/prosess/${id}`, {
      method: 'DELETE',
    });
  }

  async getStatistics(): Promise<ProsessStatistics> {
    return this.makeRequest<ProsessStatistics>('/prosess/statistics');
  }

  async getCategories(): Promise<string[]> {
    return this.makeRequest<string[]>('/prosess/categories');
  }

  async getTags(): Promise<ProsessTag[]> {
    return this.makeRequest<ProsessTag[]>('/prosess/tags');
  }
}

export const prosessService = new ProsessService();