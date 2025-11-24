export enum ApprovalStatus {
  Pending = 0,
  InProgress = 1,
  Approved = 2,
  Rejected = 3,
  Withdrawn = 4
}

export enum CommentType {
  General = 0,
  Question = 1,
  Suggestion = 2,
  Issue = 3,
  Approval = 4,
  Rejection = 5
}

export interface ProsessApprovalRequest {
  id: number;
  prosessId: number;
  prosessTitle: string;
  requestedByUserName: string;
  requestComment?: string;
  requestedAt: string;
  status: ApprovalStatus;
  approvedByUserName?: string;
  approvedAt?: string;
  approvalComment?: string;
  rejectedAt?: string;
  rejectionReason?: string;
  comments: ProsessApprovalComment[];
}

export interface ProsessApprovalComment {
  id: number;
  userId: number;
  userName: string;
  comment: string;
  createdAt: string;
  type: CommentType;
}

export interface ProsessApprovalHistory {
  id: number;
  prosessId: number;
  prosessTitle: string;
  userName: string;
  fromStatus: number;
  toStatus: number;
  comment?: string;
  changedAt: string;
  action: string;
}

export interface SubmitForApprovalRequest {
  requestComment?: string;
}

export interface ApprovalDecisionRequest {
  isApproved: boolean;
  comment?: string;
}

export interface AddApprovalCommentRequest {
  comment: string;
  type?: CommentType;
}

export interface ApprovalQueue {
  pendingApprovals: ProsessApprovalRequest[];
  inProgressApprovals: ProsessApprovalRequest[];
  recentlyCompletedApprovals: ProsessApprovalRequest[];
  statistics: ApprovalQueueStatistics;
}

export interface ApprovalQueueStatistics {
  totalPendingApprovals: number;
  totalInProgressApprovals: number;
  totalCompletedThisMonth: number;
  averageApprovalTimeHours: number;
  myPendingApprovals: number;
  myCompletedApprovals: number;
}

export interface MyApprovalRequests {
  mySubmittedRequests: ProsessApprovalRequest[];
  myCompletedRequests: ProsessApprovalRequest[];
}

export interface ApprovalNotification {
  approvalRequestId: number;
  prosessId: number;
  prosessTitle: string;
  notificationType: string;
  message: string;
  actionUrl?: string;
  createdAt: string;
  actionUserName?: string;
}