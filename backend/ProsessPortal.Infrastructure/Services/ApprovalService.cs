using Microsoft.EntityFrameworkCore;
using ProsessPortal.Core.DTOs;
using ProsessPortal.Core.Entities;
using ProsessPortal.Core.Interfaces;
using ProsessPortal.Infrastructure.Data;

namespace ProsessPortal.Infrastructure.Services;

public class ApprovalService : IApprovalService
{
    private readonly ProsessPortalDbContext _context;

    public ApprovalService(ProsessPortalDbContext context)
    {
        _context = context;
    }

    public async Task<ProsessApprovalRequestDto> SubmitForApprovalAsync(int prosessId, int userId, SubmitForApprovalRequest request)
    {
        var prosess = await _context.Prosesser
            .Include(p => p.CreatedByUser)
            .FirstOrDefaultAsync(p => p.Id == prosessId && p.IsActive);

        if (prosess == null)
            throw new ArgumentException("Prosess ikke funnet");

        if (!await CanUserSubmitForApprovalAsync(userId, prosessId))
            throw new UnauthorizedAccessException("Du har ikke tilgang til å sende denne prosessen til godkjenning");

        if (prosess.Status != ProsessStatus.Draft)
            throw new InvalidOperationException($"Kan kun sende utkast til godkjenning. Nåværende status: {prosess.Status}");

        // Check if there's already a pending approval request
        var existingRequest = await _context.ProsessApprovalRequests
            .FirstOrDefaultAsync(r => r.ProsessId == prosessId && 
                                    (r.Status == ApprovalStatus.Pending || r.Status == ApprovalStatus.InProgress));

        if (existingRequest != null)
            throw new InvalidOperationException("Det finnes allerede en aktiv godkjenningsforespørsel for denne prosessen");

        // Create approval request
        var approvalRequest = new ProsessApprovalRequest
        {
            ProsessId = prosessId,
            RequestedByUserId = userId,
            RequestComment = request.RequestComment,
            Status = ApprovalStatus.Pending
        };

        _context.ProsessApprovalRequests.Add(approvalRequest);

        // Update process status
        prosess.Status = ProsessStatus.PendingApproval;
        prosess.UpdatedAt = DateTime.UtcNow;

        // Log action
        await LogApprovalActionAsync(prosessId, userId, ProsessStatus.Draft, ProsessStatus.PendingApproval, 
            ApprovalActions.SubmitForApproval, request.RequestComment);

        await _context.SaveChangesAsync();

        return await GetApprovalRequestDtoAsync(approvalRequest.Id);
    }

    public async Task<ProsessApprovalRequestDto> ApproveProcessAsync(int approvalRequestId, int userId, ApprovalDecisionRequest request)
    {
        var approvalRequest = await _context.ProsessApprovalRequests
            .Include(r => r.Prosess)
            .Include(r => r.RequestedByUser)
            .FirstOrDefaultAsync(r => r.Id == approvalRequestId);

        if (approvalRequest == null)
            throw new ArgumentException("Godkjenningsforespørsel ikke funnet");

        if (approvalRequest.Status != ApprovalStatus.Pending && approvalRequest.Status != ApprovalStatus.InProgress)
            throw new InvalidOperationException("Denne godkjenningsforespørselen kan ikke godkjennes");

        if (!await CanUserApproveProcessAsync(userId, approvalRequest.ProsessId))
            throw new UnauthorizedAccessException("Du har ikke tilgang til å godkjenne denne prosessen");

        // Update approval request
        approvalRequest.Status = ApprovalStatus.Approved;
        approvalRequest.ApprovedByUserId = userId;
        approvalRequest.ApprovedAt = DateTime.UtcNow;
        approvalRequest.ApprovalComment = request.Comment;

        // Update process status to Published when approved
        approvalRequest.Prosess.Status = ProsessStatus.Published;
        approvalRequest.Prosess.UpdatedAt = DateTime.UtcNow;

        // Log action
        await LogApprovalActionAsync(approvalRequest.ProsessId, userId, ProsessStatus.PendingApproval, 
            ProsessStatus.Published, ApprovalActions.Approve, request.Comment);

        await _context.SaveChangesAsync();

        return await GetApprovalRequestDtoAsync(approvalRequestId);
    }

    public async Task<ProsessApprovalRequestDto> RejectProcessAsync(int approvalRequestId, int userId, ApprovalDecisionRequest request)
    {
        var approvalRequest = await _context.ProsessApprovalRequests
            .Include(r => r.Prosess)
            .Include(r => r.RequestedByUser)
            .FirstOrDefaultAsync(r => r.Id == approvalRequestId);

        if (approvalRequest == null)
            throw new ArgumentException("Godkjenningsforespørsel ikke funnet");

        if (approvalRequest.Status != ApprovalStatus.Pending && approvalRequest.Status != ApprovalStatus.InProgress)
            throw new InvalidOperationException("Denne godkjenningsforespørselen kan ikke avvises");

        if (!await CanUserApproveProcessAsync(userId, approvalRequest.ProsessId))
            throw new UnauthorizedAccessException("Du har ikke tilgang til å avvise denne prosessen");

        if (string.IsNullOrWhiteSpace(request.Comment))
            throw new ArgumentException("Kommentar er påkrevd når en prosess avvises");

        // Update approval request
        approvalRequest.Status = ApprovalStatus.Rejected;
        approvalRequest.ApprovedByUserId = userId;
        approvalRequest.RejectedAt = DateTime.UtcNow;
        approvalRequest.RejectionReason = request.Comment;

        // Update process status back to Draft
        approvalRequest.Prosess.Status = ProsessStatus.Rejected;
        approvalRequest.Prosess.UpdatedAt = DateTime.UtcNow;

        // Log action
        await LogApprovalActionAsync(approvalRequest.ProsessId, userId, ProsessStatus.PendingApproval, 
            ProsessStatus.Rejected, ApprovalActions.Reject, request.Comment);

        await _context.SaveChangesAsync();

        return await GetApprovalRequestDtoAsync(approvalRequestId);
    }

    public async Task WithdrawApprovalRequestAsync(int prosessId, int userId)
    {
        var approvalRequest = await _context.ProsessApprovalRequests
            .Include(r => r.Prosess)
            .FirstOrDefaultAsync(r => r.ProsessId == prosessId && 
                                    r.RequestedByUserId == userId &&
                                    (r.Status == ApprovalStatus.Pending || r.Status == ApprovalStatus.InProgress));

        if (approvalRequest == null)
            throw new ArgumentException("Ingen aktiv godkjenningsforespørsel funnet for denne prosessen");

        // Update approval request
        approvalRequest.Status = ApprovalStatus.Withdrawn;

        // Update process status back to Draft
        approvalRequest.Prosess.Status = ProsessStatus.Draft;
        approvalRequest.Prosess.UpdatedAt = DateTime.UtcNow;

        // Log action
        await LogApprovalActionAsync(prosessId, userId, ProsessStatus.PendingApproval, 
            ProsessStatus.Draft, ApprovalActions.Withdraw);

        await _context.SaveChangesAsync();
    }

    public async Task<ProsessApprovalCommentDto> AddCommentAsync(int approvalRequestId, int userId, AddApprovalCommentRequest request)
    {
        var approvalRequest = await _context.ProsessApprovalRequests
            .FirstOrDefaultAsync(r => r.Id == approvalRequestId);

        if (approvalRequest == null)
            throw new ArgumentException("Godkjenningsforespørsel ikke funnet");

        if (!await CanUserAccessApprovalRequestAsync(userId, approvalRequestId))
            throw new UnauthorizedAccessException("Du har ikke tilgang til denne godkjenningsforespørselen");

        var comment = new ProsessApprovalComment
        {
            ApprovalRequestId = approvalRequestId,
            UserId = userId,
            Comment = request.Comment,
            Type = request.Type
        };

        _context.ProsessApprovalComments.Add(comment);
        await _context.SaveChangesAsync();

        return await GetCommentDtoAsync(comment.Id);
    }

    public async Task<ApprovalQueueDto> GetApprovalQueueAsync(int userId)
    {
        if (!await CanUserViewApprovalQueueAsync(userId))
            throw new UnauthorizedAccessException("Du har ikke tilgang til godkjenningskøen");

        var pendingApprovals = await _context.ProsessApprovalRequests
            .Include(r => r.Prosess)
            .Include(r => r.RequestedByUser)
            .Include(r => r.Comments)
                .ThenInclude(c => c.User)
            .Where(r => r.Status == ApprovalStatus.Pending)
            .OrderBy(r => r.RequestedAt)
            .Select(r => MapToDto(r))
            .ToListAsync();

        var inProgressApprovals = await _context.ProsessApprovalRequests
            .Include(r => r.Prosess)
            .Include(r => r.RequestedByUser)
            .Include(r => r.Comments)
                .ThenInclude(c => c.User)
            .Where(r => r.Status == ApprovalStatus.InProgress)
            .OrderBy(r => r.RequestedAt)
            .Select(r => MapToDto(r))
            .ToListAsync();

        var recentlyCompleted = await _context.ProsessApprovalRequests
            .Include(r => r.Prosess)
            .Include(r => r.RequestedByUser)
            .Include(r => r.ApprovedByUser)
            .Include(r => r.Comments)
                .ThenInclude(c => c.User)
            .Where(r => (r.Status == ApprovalStatus.Approved || r.Status == ApprovalStatus.Rejected) &&
                       (r.ApprovedAt >= DateTime.UtcNow.AddDays(-7) || r.RejectedAt >= DateTime.UtcNow.AddDays(-7)))
            .OrderByDescending(r => r.ApprovedAt ?? r.RejectedAt)
            .Take(10)
            .Select(r => MapToDto(r))
            .ToListAsync();

        var statistics = await GetApprovalStatisticsAsync(userId);

        return new ApprovalQueueDto(
            pendingApprovals,
            inProgressApprovals,
            recentlyCompleted,
            statistics
        );
    }

    public async Task<bool> CanUserApproveProcessAsync(int userId, int prosessId)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

        if (user == null) return false;

        // Check if user has approve_prosess permission
        var hasApprovePermission = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Any(rp => rp.Permission.Name == PermissionNames.ApproveProsess);

        return hasApprovePermission;
    }

    public async Task<bool> CanUserSubmitForApprovalAsync(int userId, int prosessId)
    {
        var prosess = await _context.Prosesser
            .FirstOrDefaultAsync(p => p.Id == prosessId);

        if (prosess == null) return false;

        // User can submit if they are the creator or owner
        return prosess.CreatedByUserId == userId || prosess.OwnerId == userId;
    }

    public async Task LogApprovalActionAsync(int prosessId, int userId, ProsessStatus fromStatus, ProsessStatus toStatus, string action, string? comment = null)
    {
        var historyEntry = new ProsessApprovalHistory
        {
            ProsessId = prosessId,
            UserId = userId,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            Action = action,
            Comment = comment
        };

        _context.ProsessApprovalHistory.Add(historyEntry);
    }

    // Helper methods
    private async Task<ProsessApprovalRequestDto> GetApprovalRequestDtoAsync(int id)
    {
        var request = await _context.ProsessApprovalRequests
            .Include(r => r.Prosess)
            .Include(r => r.RequestedByUser)
            .Include(r => r.ApprovedByUser)
            .Include(r => r.Comments)
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        return MapToDto(request!);
    }

    private async Task<ProsessApprovalCommentDto> GetCommentDtoAsync(int id)
    {
        var comment = await _context.ProsessApprovalComments
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id);

        return new ProsessApprovalCommentDto(
            comment!.Id,
            comment.UserId,
            $"{comment.User.FirstName} {comment.User.LastName}",
            comment.Comment,
            comment.CreatedAt,
            comment.Type
        );
    }

    private static ProsessApprovalRequestDto MapToDto(ProsessApprovalRequest request)
    {
        return new ProsessApprovalRequestDto(
            request.Id,
            request.ProsessId,
            request.Prosess.Title,
            $"{request.RequestedByUser.FirstName} {request.RequestedByUser.LastName}",
            request.RequestComment,
            request.RequestedAt,
            request.Status,
            request.ApprovedByUser != null ? $"{request.ApprovedByUser.FirstName} {request.ApprovedByUser.LastName}" : null,
            request.ApprovedAt,
            request.ApprovalComment,
            request.RejectedAt,
            request.RejectionReason,
            request.Comments.Select(c => new ProsessApprovalCommentDto(
                c.Id,
                c.UserId,
                $"{c.User.FirstName} {c.User.LastName}",
                c.Comment,
                c.CreatedAt,
                c.Type
            )).ToList()
        );
    }

    private async Task<bool> CanUserViewApprovalQueueAsync(int userId)
    {
        return await CanUserApproveProcessAsync(userId, 0); // Check general approval permission
    }

    private async Task<bool> CanUserAccessApprovalRequestAsync(int userId, int approvalRequestId)
    {
        var request = await _context.ProsessApprovalRequests
            .FirstOrDefaultAsync(r => r.Id == approvalRequestId);

        if (request == null) return false;

        // User can access if they submitted it or can approve it
        return request.RequestedByUserId == userId || await CanUserApproveProcessAsync(userId, request.ProsessId);
    }

    // Implemented methods for complete functionality
    public async Task<ICollection<ProsessApprovalCommentDto>> GetCommentsAsync(int approvalRequestId)
    {
        var comments = await _context.ProsessApprovalComments
            .Include(c => c.User)
            .Where(c => c.ApprovalRequestId == approvalRequestId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        return comments.Select(c => new ProsessApprovalCommentDto(
            c.Id,
            c.UserId,
            $"{c.User.FirstName} {c.User.LastName}",
            c.Comment,
            c.CreatedAt,
            c.Type
        )).ToList();
    }

    public async Task<MyApprovalRequestsDto> GetMyApprovalRequestsAsync(int userId)
    {
        var mySubmitted = await _context.ProsessApprovalRequests
            .Include(r => r.Prosess)
            .Include(r => r.RequestedByUser)
            .Include(r => r.ApprovedByUser)
            .Include(r => r.Comments)
                .ThenInclude(c => c.User)
            .Where(r => r.RequestedByUserId == userId)
            .OrderByDescending(r => r.RequestedAt)
            .Select(r => MapToDto(r))
            .ToListAsync();

        var myCompleted = await _context.ProsessApprovalRequests
            .Include(r => r.Prosess)
            .Include(r => r.RequestedByUser)
            .Include(r => r.ApprovedByUser)
            .Include(r => r.Comments)
                .ThenInclude(c => c.User)
            .Where(r => r.ApprovedByUserId == userId)
            .OrderByDescending(r => r.ApprovedAt ?? r.RejectedAt)
            .Select(r => MapToDto(r))
            .ToListAsync();

        return new MyApprovalRequestsDto(mySubmitted, myCompleted);
    }

    public async Task<ProsessApprovalRequestDto?> GetApprovalRequestAsync(int approvalRequestId)
    {
        var request = await _context.ProsessApprovalRequests
            .Include(r => r.Prosess)
            .Include(r => r.RequestedByUser)
            .Include(r => r.ApprovedByUser)
            .Include(r => r.Comments)
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(r => r.Id == approvalRequestId);

        return request != null ? MapToDto(request) : null;
    }

    public async Task<ProsessApprovalRequestDto?> GetCurrentApprovalRequestForProcessAsync(int prosessId)
    {
        var request = await _context.ProsessApprovalRequests
            .Include(r => r.Prosess)
            .Include(r => r.RequestedByUser)
            .Include(r => r.ApprovedByUser)
            .Include(r => r.Comments)
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(r => r.ProsessId == prosessId && 
                                    (r.Status == ApprovalStatus.Pending || r.Status == ApprovalStatus.InProgress));

        return request != null ? MapToDto(request) : null;
    }

    public async Task<ICollection<ProsessApprovalHistoryDto>> GetApprovalHistoryAsync(int prosessId)
    {
        var history = await _context.ProsessApprovalHistory
            .Include(h => h.Prosess)
            .Include(h => h.User)
            .Where(h => h.ProsessId == prosessId)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync();

        return history.Select(h => new ProsessApprovalHistoryDto(
            h.Id,
            h.ProsessId,
            h.Prosess.Title,
            $"{h.User.FirstName} {h.User.LastName}",
            h.FromStatus,
            h.ToStatus,
            h.Comment,
            h.ChangedAt,
            h.Action
        )).ToList();
    }

    public async Task<bool> IsProcessInApprovalWorkflowAsync(int prosessId)
    {
        var prosess = await _context.Prosesser
            .FirstOrDefaultAsync(p => p.Id == prosessId);

        return prosess != null && 
               (prosess.Status == ProsessStatus.PendingApproval || 
                prosess.Status == ProsessStatus.InReview);
    }

    public async Task<ApprovalQueueStatisticsDto> GetApprovalStatisticsAsync(int userId)
    {
        var totalPending = await _context.ProsessApprovalRequests
            .CountAsync(r => r.Status == ApprovalStatus.Pending);

        var totalInProgress = await _context.ProsessApprovalRequests
            .CountAsync(r => r.Status == ApprovalStatus.InProgress);

        var totalCompletedThisMonth = await _context.ProsessApprovalRequests
            .CountAsync(r => (r.Status == ApprovalStatus.Approved || r.Status == ApprovalStatus.Rejected) &&
                           (r.ApprovedAt >= DateTime.UtcNow.AddMonths(-1) || r.RejectedAt >= DateTime.UtcNow.AddMonths(-1)));

        var completedRequests = await _context.ProsessApprovalRequests
            .Where(r => r.Status == ApprovalStatus.Approved || r.Status == ApprovalStatus.Rejected)
            .Where(r => r.ApprovedAt.HasValue || r.RejectedAt.HasValue)
            .Select(r => new { 
                RequestedAt = r.RequestedAt, 
                CompletedAt = r.ApprovedAt ?? r.RejectedAt!.Value 
            })
            .ToListAsync();

        double averageHours = 0;
        if (completedRequests.Count > 0)
        {
            var totalHours = completedRequests
                .Select(r => (r.CompletedAt - r.RequestedAt).TotalHours)
                .Sum();
            averageHours = totalHours / completedRequests.Count;
        }

        var myPending = await _context.ProsessApprovalRequests
            .CountAsync(r => r.RequestedByUserId == userId && 
                           (r.Status == ApprovalStatus.Pending || r.Status == ApprovalStatus.InProgress));

        var myCompleted = await _context.ProsessApprovalRequests
            .CountAsync(r => r.ApprovedByUserId == userId && 
                           (r.Status == ApprovalStatus.Approved || r.Status == ApprovalStatus.Rejected));

        return new ApprovalQueueStatisticsDto(
            totalPending,
            totalInProgress,
            totalCompletedThisMonth,
            averageHours,
            myPending,
            myCompleted
        );
    }

    public async Task<ICollection<ApprovalNotificationDto>> GetApprovalNotificationsAsync(int userId)
    {
        // For now, return empty list - notifications would require additional implementation
        await Task.CompletedTask;
        return new List<ApprovalNotificationDto>();
    }

    public async Task MarkNotificationsAsReadAsync(int userId, ICollection<int> notificationIds)
    {
        // For now, do nothing - notifications would require additional implementation
        await Task.CompletedTask;
    }
}