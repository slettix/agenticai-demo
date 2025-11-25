export enum ProsessStatus {
  Draft = 0,
  PendingApproval = 1,
  InReview = 2,
  Approved = 3,
  Rejected = 4,
  Published = 5,
  Deprecated = 6,
  Archived = 7
}

export enum StepType {
  Start = 0,
  Task = 1,
  Decision = 2,
  Document = 3,
  Approval = 4,
  Gateway = 5,
  Review = 6,
  Wait = 7,
  End = 8,
  Subprocess = 9
}

export enum ConnectionType {
  Sequential = 0,
  Conditional = 1,
  Parallel = 2,
  Loop = 3,
  Exception = 4
}

export interface ProsessListItem {
  id: number;
  title: string;
  description: string;
  category: string;
  status: ProsessStatus;
  updatedAt: string;
  createdByUserName: string;
  ownerName?: string;
  viewCount: number;
  tags: string[];
}

export interface ProsessDetail {
  id: number;
  title: string;
  description: string;
  category: string;
  status: ProsessStatus;
  gitRepository?: string;
  gitPath?: string;
  gitBranch?: string;
  createdAt: string;
  updatedAt: string;
  createdByUserName: string;
  ownerName?: string;
  viewCount: number;
  lastAccessedAt?: string;
  tags: ProsessTag[];
  steps: ProsessStep[];
  currentVersion?: ProsessVersion;
  versionHistory: ProsessVersionSummary[];
}

export interface ProsessStep {
  id: number;
  title: string;
  description: string;
  detailedInstructions?: string;
  orderIndex: number;
  type: StepType;
  responsibleRole?: string;
  estimatedDurationMinutes?: number;
  isOptional: boolean;
  parentStepId?: number;
  subSteps: ProsessStep[];
  outgoingConnections: StepConnection[];
}

export interface StepConnection {
  id: number;
  toStepId: number;
  condition?: string;
  type: ConnectionType;
}

export interface ProsessTag {
  id: number;
  name: string;
  color: string;
}

export interface ProsessVersion {
  id: number;
  versionNumber: string;
  title: string;
  description: string;
  content: string;
  changeLog: string;
  createdAt: string;
  createdByUserName: string;
  isCurrent: boolean;
  isPublished: boolean;
  publishedAt?: string;
  publishedByUserName?: string;
}

export interface ProsessVersionSummary {
  id: number;
  versionNumber: string;
  title: string;
  createdAt: string;
  createdByUserName: string;
  isCurrent: boolean;
  isPublished: boolean;
}

export interface CreateProsessRequest {
  title: string;
  description: string;
  category: string;
  itilArea?: string;
  priority?: string;
  ownerId?: number;
  tags?: string[];
  processSteps?: CreateProsessStepRequest[];
}

export interface CreateProsessStepRequest {
  title: string;
  description: string;
  type: StepType;
  responsibleRole?: string;
  estimatedDurationMinutes?: number;
  orderIndex: number;
  isOptional?: boolean;
  detailedInstructions?: string;
  itilGuidance?: string;
}

export interface UpdateProsessRequest {
  title: string;
  description: string;
  category: string;
  itilArea?: string;
  priority?: string;
  ownerId?: number;
  tags?: string[];
}

export interface ProsessSearchRequest {
  search?: string;
  category?: string;
  status?: ProsessStatus;
  tag?: string;
  createdBy?: string;
  owner?: string;
  createdAfter?: string;
  createdBefore?: string;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ProsessStatistics {
  totalProsesses: number;
  publishedProsesses: number;
  draftProsesses: number;
  inReviewProsesses: number;
  processesByCategory: Record<string, number>;
  processesByStatus: Record<string, number>;
  totalViews: number;
  mostViewedProsess?: ProsessListItem;
  recentlyUpdatedProsess?: ProsessListItem;
}