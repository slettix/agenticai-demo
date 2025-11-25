export interface DeleteProsessRequest {
  reason: string;
  forceDelete?: boolean;
}

export interface RestoreProsessRequest {
  reason: string;
}

export interface DeletedProsessDto {
  id: number;
  title: string;
  description: string;
  category: string;
  status: ProsessStatus;
  deletedAt: string;
  deletedByUser: string;
  reason?: string;
  canRestore: boolean;
}

export interface DeletionHistoryDto {
  id: number;
  prosessId: number;
  prosessTitle: string;
  deletedByUser: string;
  deletedAt: string;
  reason?: string;
  restoredAt?: string;
  restoredByUser?: string;
  restoreReason?: string;
}

export interface BulkDeleteRequest {
  prosessIds: number[];
  reason: string;
  forceDelete?: boolean;
}

export interface BulkDeleteResult {
  totalRequested: number;
  successfullyDeleted: number;
  failed: number;
  errors: string[];
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export enum ProsessStatus {
  Draft = 0,
  PendingApproval = 1,
  InReview = 2,
  Approved = 3,
  Rejected = 4,
  Published = 5,
  Deprecated = 6,
  Archived = 7,
  Deleted = 8
}