import { ProsessDetail, CreateProsessStepRequest } from './prosess';

export enum ProsessEditStatus {
  Active = 0,
  Idle = 1,
  Completed = 2,
  Cancelled = 3,
  ConflictDetected = 4,
  Expired = 5
}

export enum VersionChangeType {
  Patch = 0,
  Minor = 1,
  Major = 2
}

export enum ProsessChangeType {
  Added = 0,
  Modified = 1,
  Deleted = 2,
  Moved = 3,
  NoChange = 4
}

export enum ConflictResolutionType {
  KeepMine = 0,
  KeepTheirs = 1,
  Merge = 2,
  Cancel = 3
}

export interface ProsessEditSession {
  prosessId: number;
  sessionId: string;
  userId: number;
  userName: string;
  startedAt: string;
  lastActivity: string;
  isActive: boolean;
  status: ProsessEditStatus;
}

export interface StartEditSessionRequest {
  comment?: string;
}

export interface StartEditSessionResponse {
  sessionId: string;
  prosess: ProsessDetail;
  editSession: ProsessEditSession;
  activeSessions: ProsessEditSession[];
}

export interface EditProsessRequest {
  title: string;
  description: string;
  category: string;
  itilArea?: string;
  priority?: string;
  ownerId?: number;
  tags?: string[];
  steps?: CreateProsessStepRequest[];
  saveAsDraft?: boolean;
  changeComment?: string;
  versionChangeType?: VersionChangeType;
}

export interface SaveDraftRequest {
  title: string;
  description: string;
  category: string;
  itilArea?: string;
  priority?: string;
  ownerId?: number;
  tags?: string[];
  steps?: CreateProsessStepRequest[];
  autoSaveComment?: string;
}

export interface ProsessDiff {
  fromVersionId: number;
  toVersionId: number;
  fromVersionNumber: string;
  toVersionNumber: string;
  fromDate: string;
  toDate: string;
  fromCreatedBy: string;
  toCreatedBy: string;
  changes: ProsessFieldChange[];
}

export interface ProsessFieldChange {
  fieldName: string;
  displayName: string;
  oldValue?: string;
  newValue?: string;
  changeType: ProsessChangeType;
}

export interface ProsessVersionCompareRequest {
  fromVersionId: number;
  toVersionId: number;
}

export interface EditConflict {
  prosessId: number;
  conflictingSessionId: string;
  conflictingUserName: string;
  conflictStarted: string;
  message: string;
  conflictingFields: string[];
}

export interface ResolveConflictRequest {
  sessionId: string;
  resolution: ConflictResolutionType;
  fieldsToKeep?: string[];
  message?: string;
}

export interface ProsessLock {
  prosessId: number;
  lockedBySessionId: string;
  lockedByUserName: string;
  lockedAt: string;
  expiresAt: string;
  isEditable: boolean;
}

export interface EditingStatistics {
  activeEditSessions: number;
  draftVersions: number;
  completedEditsToday: number;
  averageEditTimeMinutes: number;
  myActiveSessions: ProsessEditSession[];
  recentlyCompleted: ProsessEditSession[];
}

export interface AutoSaveData {
  sessionId: string;
  lastSaved: string;
  hasUnsavedChanges: boolean;
  autoSaveEnabled: boolean;
  nextAutoSave?: string;
}