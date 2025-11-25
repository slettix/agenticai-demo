using ProsessPortal.Core.Entities;

namespace ProsessPortal.Core.DTOs;

public record DeleteProsessRequest(
    string Reason,
    bool ForceDelete = false
);

public record RestoreProsessRequest(
    string Reason
);

public record DeletedProsessDto(
    int Id,
    string Title,
    string Description,
    string Category,
    ProsessStatus Status,
    DateTime DeletedAt,
    string DeletedByUser,
    string? Reason,
    bool CanRestore
);

public record DeletionHistoryDto(
    int Id,
    int ProsessId,
    string ProsessTitle,
    string DeletedByUser,
    DateTime DeletedAt,
    string? Reason,
    DateTime? RestoredAt,
    string? RestoredByUser,
    string? RestoreReason
);

public record BulkDeleteRequest(
    ICollection<int> ProsessIds,
    string Reason,
    bool ForceDelete = false
);

public record BulkDeleteResult(
    int TotalRequested,
    int SuccessfullyDeleted,
    int Failed,
    ICollection<string> Errors
);

public record DeletedProsessesListDto(
    ICollection<DeletedProsessDto> DeletedProcesses,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);