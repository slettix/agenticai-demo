export enum RoleCategory {
  Internal = 0,
  External = 1,
  System = 2
}

export enum OrganizationLevel {
  Individual = 0,
  Unit = 1,
  Department = 2,
  Organization = 3,
  National = 4
}

export interface Permission {
  id: number;
  name: string;
  description: string;
  resource: string;
  action: string;
}

export interface Role {
  id: number;
  name: string;
  description: string;
  category: RoleCategory;
  level: OrganizationLevel;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
  createdByUserName: string;
  updatedByUserName?: string;
  permissions?: Permission[];
}

export interface CreateRole {
  name: string;
  description: string;
  category: RoleCategory;
  level: OrganizationLevel;
  permissionIds?: number[];
}

export interface UpdateRole {
  name: string;
  description: string;
  category: RoleCategory;
  level: OrganizationLevel;
  isActive: boolean;
  permissionIds?: number[];
}

export interface RoleSearch {
  searchTerm?: string;
  category?: RoleCategory;
  level?: OrganizationLevel;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
}

export interface RoleList {
  roles: Role[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface RoleCategories {
  categories: { [key: number]: string };
  levels: { [key: number]: string };
}