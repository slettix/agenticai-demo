using ProsessPortal.Core.Entities;

namespace ProsessPortal.Core.DTOs;

public record EditProsessRequest(
    string Title,
    string Description,
    string Category,
    string? ITILArea = null,
    string? Priority = null,
    int? OwnerId = null,
    ICollection<string>? Tags = null,
    ICollection<CreateProsessStepRequest>? Steps = null,
    bool SaveAsDraft = false,
    string? ChangeComment = null,
    VersionChangeType VersionChangeType = VersionChangeType.Minor
);

public record ProsessEditSessionDto(
    int ProsessId,
    string SessionId,
    int UserId,
    string UserName,
    DateTime StartedAt,
    DateTime LastActivity,
    bool IsActive,
    ProsessEditStatus Status
);

public record StartEditSessionRequest(
    string? Comment = null
);

public record StartEditSessionResponse(
    string SessionId,
    ProsessDetailDto Prosess,
    ProsessEditSessionDto EditSession,
    ICollection<ProsessEditSessionDto> ActiveSessions
);

public record SaveDraftRequest(
    string Title,
    string Description,
    string Category,
    string? ITILArea = null,
    string? Priority = null,
    int? OwnerId = null,
    ICollection<string>? Tags = null,
    ICollection<CreateProsessStepRequest>? Steps = null,
    string? AutoSaveComment = null
);

public record ProsessDiffDto(
    int FromVersionId,
    int ToVersionId,
    string FromVersionNumber,
    string ToVersionNumber,
    DateTime FromDate,
    DateTime ToDate,
    string FromCreatedBy,
    string ToCreatedBy,
    ICollection<ProsessFieldChangeDto> Changes
);

public record ProsessFieldChangeDto(
    string FieldName,
    string DisplayName,
    string? OldValue,
    string? NewValue,
    ProsessChangeType ChangeType
);

public record ProsessVersionCompareRequest(
    int FromVersionId,
    int ToVersionId
);

public record EditConflictDto(
    int ProsessId,
    string ConflictingSessionId,
    string ConflictingUserName,
    DateTime ConflictStarted,
    string Message,
    ICollection<string> ConflictingFields
);

public record ResolveConflictRequest(
    string SessionId,
    ConflictResolutionType Resolution,
    ICollection<string>? FieldsToKeep = null,
    string? Message = null
);

public record ProsessLockDto(
    int ProsessId,
    string LockedBySessionId,
    string LockedByUserName,
    DateTime LockedAt,
    DateTime ExpiresAt,
    bool IsEditable
);

public record EditingStatisticsDto(
    int ActiveEditSessions,
    int DraftVersions,
    int CompletedEditsToday,
    double AverageEditTimeMinutes,
    ICollection<ProsessEditSessionDto> MyActiveSessions,
    ICollection<ProsessEditSessionDto> RecentlyCompleted
);

public enum ProsessChangeType
{
    Added = 0,
    Modified = 1,
    Deleted = 2,
    Moved = 3,
    NoChange = 4
}