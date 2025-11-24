namespace ProsessPortal.Core.Entities;

public class ProsessEditSession
{
    public int Id { get; set; }
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    public int ProsessId { get; set; }
    public Prosess Prosess { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public ProsessEditStatus Status { get; set; } = ProsessEditStatus.Active;
    public string? StartComment { get; set; }
    public string? CompletionComment { get; set; }
    public bool IsLocked { get; set; } = false;
    public DateTime? LockExpiresAt { get; set; }
    
    // Auto-save data
    public string? DraftTitle { get; set; }
    public string? DraftDescription { get; set; }
    public string? DraftCategory { get; set; }
    public string? DraftTags { get; set; } // JSON string
    public string? DraftSteps { get; set; } // JSON string
    public DateTime? LastAutoSave { get; set; }
    
    // Version tracking
    public int? CreatedVersionId { get; set; }
    public ProsessVersion? CreatedVersion { get; set; }
}

public class ProsessEditConflict
{
    public int Id { get; set; }
    public int ProsessId { get; set; }
    public Prosess Prosess { get; set; } = null!;
    public string SessionId1 { get; set; } = string.Empty;
    public string SessionId2 { get; set; } = string.Empty;
    public int UserId1 { get; set; }
    public User User1 { get; set; } = null!;
    public int UserId2 { get; set; }
    public User User2 { get; set; } = null!;
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
    public string ConflictingFields { get; set; } = string.Empty; // JSON array
    public ConflictResolutionType? Resolution { get; set; }
    public int? ResolvedByUserId { get; set; }
    public User? ResolvedByUser { get; set; }
    public string? ResolutionComment { get; set; }
}

public class ProsessAutoSave
{
    public int Id { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public int ProsessId { get; set; }
    public Prosess Prosess { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string Content { get; set; } = string.Empty; // JSON of the auto-saved data
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
    public bool IsRestored { get; set; } = false;
}

public enum ProsessEditStatus
{
    Active = 0,
    Idle = 1,
    Completed = 2,
    Cancelled = 3,
    ConflictDetected = 4,
    Expired = 5
}

public enum ConflictResolutionType
{
    KeepMine = 0,
    KeepTheirs = 1,
    Merge = 2,
    Cancel = 3
}

public static class EditingConstants
{
    public const int SessionTimeoutMinutes = 30;
    public const int LockTimeoutMinutes = 15;
    public const int AutoSaveIntervalSeconds = 30;
    public const int MaxConcurrentSessions = 5;
}