namespace ProsessPortal.Core.Entities;

public class ProsessApprovalRequest
{
    public int Id { get; set; }
    public int ProsessId { get; set; }
    public Prosess Prosess { get; set; } = null!;
    public int RequestedByUserId { get; set; }
    public User RequestedByUser { get; set; } = null!;
    public string? RequestComment { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
    public int? ApprovedByUserId { get; set; }
    public User? ApprovedByUser { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovalComment { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }
    
    // Navigation properties
    public ICollection<ProsessApprovalComment> Comments { get; set; } = new List<ProsessApprovalComment>();
}

public class ProsessApprovalComment
{
    public int Id { get; set; }
    public int ApprovalRequestId { get; set; }
    public ProsessApprovalRequest ApprovalRequest { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public CommentType Type { get; set; } = CommentType.General;
}

public class ProsessApprovalHistory
{
    public int Id { get; set; }
    public int ProsessId { get; set; }
    public Prosess Prosess { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public ProsessStatus FromStatus { get; set; }
    public ProsessStatus ToStatus { get; set; }
    public string? Comment { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string Action { get; set; } = string.Empty; // "submit_for_approval", "approve", "reject", "withdraw"
}

public enum ApprovalStatus
{
    Pending = 0,
    InProgress = 1,
    Approved = 2,
    Rejected = 3,
    Withdrawn = 4
}

public enum CommentType
{
    General = 0,
    Question = 1,
    Suggestion = 2,
    Issue = 3,
    Approval = 4,
    Rejection = 5
}

public static class ApprovalActions
{
    public const string SubmitForApproval = "submit_for_approval";
    public const string Approve = "approve";
    public const string Reject = "reject";
    public const string Withdraw = "withdraw";
    public const string RequestChanges = "request_changes";
}