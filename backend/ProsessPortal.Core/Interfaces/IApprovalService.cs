using ProsessPortal.Core.DTOs;
using ProsessPortal.Core.Entities;

namespace ProsessPortal.Core.Interfaces;

public interface IApprovalService
{
    // Submission and withdrawal
    Task<ProsessApprovalRequestDto> SubmitForApprovalAsync(int prosessId, int userId, SubmitForApprovalRequest request);
    Task WithdrawApprovalRequestAsync(int prosessId, int userId);
    
    // Approval decisions
    Task<ProsessApprovalRequestDto> ApproveProcessAsync(int approvalRequestId, int userId, ApprovalDecisionRequest request);
    Task<ProsessApprovalRequestDto> RejectProcessAsync(int approvalRequestId, int userId, ApprovalDecisionRequest request);
    
    // Comments
    Task<ProsessApprovalCommentDto> AddCommentAsync(int approvalRequestId, int userId, AddApprovalCommentRequest request);
    Task<ICollection<ProsessApprovalCommentDto>> GetCommentsAsync(int approvalRequestId);
    
    // Queues and listings
    Task<ApprovalQueueDto> GetApprovalQueueAsync(int userId);
    Task<MyApprovalRequestsDto> GetMyApprovalRequestsAsync(int userId);
    Task<ProsessApprovalRequestDto?> GetApprovalRequestAsync(int approvalRequestId);
    Task<ProsessApprovalRequestDto?> GetCurrentApprovalRequestForProcessAsync(int prosessId);
    
    // History and audit
    Task<ICollection<ProsessApprovalHistoryDto>> GetApprovalHistoryAsync(int prosessId);
    Task LogApprovalActionAsync(int prosessId, int userId, ProsessStatus fromStatus, ProsessStatus toStatus, string action, string? comment = null);
    
    // Permissions and validation
    Task<bool> CanUserApproveProcessAsync(int userId, int prosessId);
    Task<bool> CanUserSubmitForApprovalAsync(int userId, int prosessId);
    Task<bool> IsProcessInApprovalWorkflowAsync(int prosessId);
    
    // Statistics
    Task<ApprovalQueueStatisticsDto> GetApprovalStatisticsAsync(int userId);
    
    // Notifications
    Task<ICollection<ApprovalNotificationDto>> GetApprovalNotificationsAsync(int userId);
    Task MarkNotificationsAsReadAsync(int userId, ICollection<int> notificationIds);
}