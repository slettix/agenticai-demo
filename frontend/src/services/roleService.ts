import { Role, CreateRole, UpdateRole, RoleSearch, RoleList, Permission, RoleCategories } from '../types/role.ts';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5001';

class RoleService {
  private async getAuthToken(): Promise<string> {
    const token = localStorage.getItem('prosessportal_token');
    if (!token) {
      throw new Error('Ikke innlogget');
    }
    return token;
  }

  private async makeRequest<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    try {
      const token = await this.getAuthToken();
      
      const url = `${API_BASE_URL}/api/role${endpoint}`;
      console.log('RoleService: Making request to', url);
      
      const response = await fetch(url, {
        ...options,
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`,
          ...options.headers,
        },
      });

      console.log('RoleService: Response status', response.status);

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.error || `HTTP error! status: ${response.status}`);
      }

      // Handle DELETE responses that return 204 No Content
      if (response.status === 204 || !response.headers.get('content-type')?.includes('application/json')) {
        return undefined as unknown as T;
      }

      return response.json();
    } catch (error) {
      console.error('RoleService: Request failed', error);
      if (error instanceof TypeError && error.message === 'Failed to fetch') {
        throw new Error('Kan ikke nå serveren. Sjekk at backend kjører på http://localhost:5001');
      }
      throw error;
    }
  }

  // Role CRUD operations
  async getRoles(search: RoleSearch = {}): Promise<RoleList> {
    const params = new URLSearchParams();
    
    if (search.searchTerm) params.append('searchTerm', search.searchTerm);
    if (search.category !== undefined) params.append('category', search.category.toString());
    if (search.level !== undefined) params.append('level', search.level.toString());
    if (search.isActive !== undefined) params.append('isActive', search.isActive.toString());
    if (search.page) params.append('page', search.page.toString());
    if (search.pageSize) params.append('pageSize', search.pageSize.toString());
    
    const queryString = params.toString() ? `?${params.toString()}` : '';
    
    return this.makeRequest<RoleList>(`${queryString}`);
  }

  async getRole(id: number): Promise<Role> {
    return this.makeRequest<Role>(`/${id}`);
  }

  async createRole(createRole: CreateRole): Promise<Role> {
    return this.makeRequest<Role>('', {
      method: 'POST',
      body: JSON.stringify(createRole),
    });
  }

  async updateRole(id: number, updateRole: UpdateRole): Promise<Role> {
    return this.makeRequest<Role>(`/${id}`, {
      method: 'PUT',
      body: JSON.stringify(updateRole),
    });
  }

  async deleteRole(id: number): Promise<void> {
    await this.makeRequest<void>(`/${id}`, {
      method: 'DELETE',
    });
  }

  async activateRole(id: number): Promise<void> {
    await this.makeRequest<void>(`/${id}/activate`, {
      method: 'POST',
    });
  }

  async deactivateRole(id: number): Promise<void> {
    await this.makeRequest<void>(`/${id}/deactivate`, {
      method: 'POST',
    });
  }

  // Permission operations
  async getAllPermissions(): Promise<Permission[]> {
    return this.makeRequest<Permission[]>('/permissions');
  }

  async getRolePermissions(roleId: number): Promise<Permission[]> {
    return this.makeRequest<Permission[]>(`/${roleId}/permissions`);
  }

  async assignPermissionToRole(roleId: number, permissionId: number): Promise<void> {
    await this.makeRequest<void>(`/${roleId}/permissions/${permissionId}`, {
      method: 'POST',
    });
  }

  async removePermissionFromRole(roleId: number, permissionId: number): Promise<void> {
    await this.makeRequest<void>(`/${roleId}/permissions/${permissionId}`, {
      method: 'DELETE',
    });
  }

  // Helper methods
  async getRoleCategories(): Promise<RoleCategories> {
    return this.makeRequest<RoleCategories>('/categories');
  }

  // Label helper functions
  getRoleCategoryLabel(category: number): string {
    switch (category) {
      case 0: return 'Intern';
      case 1: return 'Ekstern';
      case 2: return 'System';
      default: return 'Ukjent';
    }
  }

  getOrganizationLevelLabel(level: number): string {
    switch (level) {
      case 0: return 'Individuell';
      case 1: return 'Enhet';
      case 2: return 'Avdeling';
      case 3: return 'Organisasjon';
      case 4: return 'Nasjonalt';
      default: return 'Ukjent';
    }
  }
}

export const roleService = new RoleService();