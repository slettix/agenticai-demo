using ProsessPortal.Core.Entities;

namespace ProsessPortal.Core.DTOs;

public record ProsessApprovalRequestDto(
    int Id,
    int ProsessId,
    string ProsessTitle,
    string RequestedByUserName,
    string? RequestComment,
    DateTime RequestedAt,
    ApprovalStatus Status,
    string? ApprovedByUserName,
    DateTime? ApprovedAt,
    string? ApprovalComment,
    DateTime? RejectedAt,
    string? RejectionReason,
    ICollection<ProsessApprovalCommentDto> Comments
);

public record ProsessApprovalCommentDto(
    int Id,
    int UserId,
    string UserName,
    string Comment,
    DateTime CreatedAt,
    CommentType Type
);

public record ProsessApprovalHistoryDto(
    int Id,
    int ProsessId,
    string ProsessTitle,
    string UserName,
    ProsessStatus FromStatus,
    ProsessStatus ToStatus,
    string? Comment,
    DateTime ChangedAt,
    string Action
);

public record SubmitForApprovalRequest(
    string? RequestComment = null
);

public record ApprovalDecisionRequest(
    bool IsApproved,
    string? Comment = null
);

public record AddApprovalCommentRequest(
    string Comment,
    CommentType Type = CommentType.General
);

public record ApprovalQueueDto(
    ICollection<ProsessApprovalRequestDto> PendingApprovals,
    ICollection<ProsessApprovalRequestDto> InProgressApprovals,
    ICollection<ProsessApprovalRequestDto> RecentlyCompletedApprovals,
    ApprovalQueueStatisticsDto Statistics
);

public record ApprovalQueueStatisticsDto(
    int TotalPendingApprovals,
    int TotalInProgressApprovals,
    int TotalCompletedThisMonth,
    double AverageApprovalTimeHours,
    int MyPendingApprovals,
    int MyCompletedApprovals
);

public record MyApprovalRequestsDto(
    ICollection<ProsessApprovalRequestDto> MySubmittedRequests,
    ICollection<ProsessApprovalRequestDto> MyCompletedRequests
);

public record ApprovalNotificationDto(
    int ApprovalRequestId,
    int ProsessId,
    string ProsessTitle,
    string NotificationType, // "submitted", "approved", "rejected", "comment_added"
    string Message,
    string? ActionUrl,
    DateTime CreatedAt,
    string? ActionUserName
);