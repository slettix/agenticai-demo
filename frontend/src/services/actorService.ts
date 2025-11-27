// Actor Types
export enum ActorCategory {
  Person = 0,
  Organization = 1,
  Unit = 2
}

export enum ActorType {
  Internal = 0,
  External = 1,
  Contractor = 2,
  Partner = 3,
  Vendor = 4
}

export enum SecurityClearance {
  None = 0,
  Restricted = 1,
  Confidential = 2,
  Secret = 3,
  TopSecret = 4
}

export interface Actor {
  id: number;
  actorCategory: ActorCategory;
  
  // Personal information (for Person category)
  firstName?: string;
  lastName?: string;
  
  // Organization/Unit information
  organizationName?: string;
  unitName?: string;
  unitType?: string;
  unitCode?: string;
  
  // Common fields
  email: string;
  phone?: string;
  actorType: ActorType;
  securityClearance: SecurityClearance;
  department?: string;
  position?: string;
  managerName?: string;
  managerEmail?: string;
  geographicLocation?: string;
  address?: string;
  preferredLanguage?: string;
  contractNumber?: string;
  contractStartDate?: string;
  contractEndDate?: string;
  vendorId?: string;
  
  // Organization-specific fields
  registrationNumber?: string;
  parentOrganization?: string;
  employeeCount?: number;
  
  // Unit-specific fields
  commandStructure?: string;
  unitMission?: string;
  personnelCount?: number;
  
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
  createdByUserName: string;
  updatedByUserName?: string;
  displayName: string;
  assignedRoles?: RoleAssignment[];
  notes?: ActorNote[];
}

export interface CreateActor {
  actorCategory: ActorCategory;
  
  // Personal information (required for Person category)
  firstName?: string;
  lastName?: string;
  
  // Organization/Unit information
  organizationName?: string;
  unitName?: string;
  unitType?: string;
  unitCode?: string;
  
  // Common fields
  email: string;
  phone?: string;
  actorType: ActorType;
  securityClearance: SecurityClearance;
  department?: string;
  position?: string;
  managerName?: string;
  managerEmail?: string;
  geographicLocation?: string;
  address?: string;
  preferredLanguage?: string;
  contractNumber?: string;
  contractStartDate?: string;
  contractEndDate?: string;
  vendorId?: string;
  
  // Organization-specific fields
  registrationNumber?: string;
  parentOrganization?: string;
  employeeCount?: number;
  
  // Unit-specific fields
  commandStructure?: string;
  unitMission?: string;
  personnelCount?: number;
}

export interface UpdateActor {
  actorCategory: ActorCategory;
  
  // Personal information
  firstName?: string;
  lastName?: string;
  
  // Organization/Unit information
  organizationName?: string;
  unitName?: string;
  unitType?: string;
  unitCode?: string;
  
  // Common fields
  email: string;
  phone?: string;
  actorType: ActorType;
  securityClearance: SecurityClearance;
  department?: string;
  position?: string;
  managerName?: string;
  managerEmail?: string;
  geographicLocation?: string;
  address?: string;
  preferredLanguage?: string;
  contractNumber?: string;
  contractStartDate?: string;
  contractEndDate?: string;
  vendorId?: string;
  
  // Organization-specific fields
  registrationNumber?: string;
  parentOrganization?: string;
  employeeCount?: number;
  
  // Unit-specific fields
  commandStructure?: string;
  unitMission?: string;
  personnelCount?: number;
  
  isActive: boolean;
}

export interface ActorSearch {
  searchTerm?: string;
  actorCategory?: ActorCategory;
  actorType?: ActorType;
  securityClearance?: SecurityClearance;
  organizationName?: string;
  unitName?: string;
  department?: string;
  geographicLocation?: string;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
}

export interface ActorList {
  actors: Actor[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface RoleAssignment {
  roleId: number;
  roleName: string;
  roleDescription?: string;
  assignedAt: string;
  assignedByUserName: string;
  validFrom?: string;
  validTo?: string;
  isActive: boolean;
  notes?: string;
}

export interface AssignRoleToActor {
  roleId: number;
  validFrom?: string;
  validTo?: string;
  notes?: string;
}

export interface ActorNote {
  id: number;
  note: string;
  category?: string;
  createdAt: string;
  createdByUserName: string;
  isPrivate: boolean;
}

export interface CreateActorNote {
  note: string;
  category?: string;
  isPrivate?: boolean;
}

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5001';

class ActorService {
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
      
      const url = `${API_BASE_URL}/api/actor${endpoint}`;
      console.log('ActorService: Making request to', url);
      
      const response = await fetch(url, {
        ...options,
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`,
          ...options.headers,
        },
      });

      console.log('ActorService: Response status', response.status);

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
      console.error('ActorService: Request failed', error);
      if (error instanceof TypeError && error.message === 'Failed to fetch') {
        throw new Error('Kan ikke nå serveren. Sjekk at backend kjører på http://localhost:5001');
      }
      throw error;
    }
  }

  // Actor CRUD operations
  async getActors(search: ActorSearch = {}): Promise<ActorList> {
    const params = new URLSearchParams();
    
    if (search.searchTerm) params.append('searchTerm', search.searchTerm);
    if (search.actorCategory !== undefined) params.append('actorCategory', search.actorCategory.toString());
    if (search.actorType !== undefined) params.append('actorType', search.actorType.toString());
    if (search.securityClearance !== undefined) params.append('securityClearance', search.securityClearance.toString());
    if (search.organizationName) params.append('organizationName', search.organizationName);
    if (search.unitName) params.append('unitName', search.unitName);
    if (search.department) params.append('department', search.department);
    if (search.geographicLocation) params.append('geographicLocation', search.geographicLocation);
    if (search.isActive !== undefined) params.append('isActive', search.isActive.toString());
    if (search.page) params.append('page', search.page.toString());
    if (search.pageSize) params.append('pageSize', search.pageSize.toString());
    
    const queryString = params.toString() ? `?${params.toString()}` : '';
    
    return this.makeRequest<ActorList>(`${queryString}`);
  }

  async getActor(id: number): Promise<Actor> {
    return this.makeRequest<Actor>(`/${id}`);
  }

  async getActorByEmail(email: string): Promise<Actor> {
    return this.makeRequest<Actor>(`/email/${encodeURIComponent(email)}`);
  }

  async createActor(createActor: CreateActor): Promise<Actor> {
    return this.makeRequest<Actor>('', {
      method: 'POST',
      body: JSON.stringify(createActor),
    });
  }

  async updateActor(id: number, updateActor: UpdateActor): Promise<Actor> {
    return this.makeRequest<Actor>(`/${id}`, {
      method: 'PUT',
      body: JSON.stringify(updateActor),
    });
  }

  async deleteActor(id: number): Promise<void> {
    await this.makeRequest<void>(`/${id}`, {
      method: 'DELETE',
    });
  }

  async activateActor(id: number): Promise<void> {
    await this.makeRequest<void>(`/${id}/activate`, {
      method: 'POST',
    });
  }

  async deactivateActor(id: number): Promise<void> {
    await this.makeRequest<void>(`/${id}/deactivate`, {
      method: 'POST',
    });
  }

  // Role assignment operations
  async getActorRoles(actorId: number): Promise<RoleAssignment[]> {
    return this.makeRequest<RoleAssignment[]>(`/${actorId}/roles`);
  }

  async assignRoleToActor(actorId: number, assignRole: AssignRoleToActor): Promise<void> {
    await this.makeRequest<void>(`/${actorId}/roles`, {
      method: 'POST',
      body: JSON.stringify(assignRole),
    });
  }

  async removeRoleFromActor(actorId: number, roleId: number): Promise<void> {
    await this.makeRequest<void>(`/${actorId}/roles/${roleId}`, {
      method: 'DELETE',
    });
  }

  async updateRoleAssignment(actorId: number, roleId: number, updateRole: AssignRoleToActor): Promise<void> {
    await this.makeRequest<void>(`/${actorId}/roles/${roleId}`, {
      method: 'PUT',
      body: JSON.stringify(updateRole),
    });
  }

  // Notes operations
  async getActorNotes(actorId: number, includePrivate: boolean = false): Promise<ActorNote[]> {
    const params = includePrivate ? '?includePrivate=true' : '';
    return this.makeRequest<ActorNote[]>(`/${actorId}/notes${params}`);
  }

  async addActorNote(actorId: number, createNote: CreateActorNote): Promise<ActorNote> {
    return this.makeRequest<ActorNote>(`/${actorId}/notes`, {
      method: 'POST',
      body: JSON.stringify(createNote),
    });
  }

  async deleteActorNote(noteId: number): Promise<void> {
    await this.makeRequest<void>(`/notes/${noteId}`, {
      method: 'DELETE',
    });
  }

  // Search and filter helpers
  async getOrganizations(): Promise<string[]> {
    return this.makeRequest<string[]>('/organizations');
  }

  async getDepartments(): Promise<string[]> {
    return this.makeRequest<string[]>('/departments');
  }

  // Statistics
  async getActorTypeStats(): Promise<Record<string, number>> {
    return this.makeRequest<Record<string, number>>('/stats/actor-types');
  }

  async getSecurityClearanceStats(): Promise<Record<string, number>> {
    return this.makeRequest<Record<string, number>>('/stats/security-clearances');
  }
}

export const actorService = new ActorService();