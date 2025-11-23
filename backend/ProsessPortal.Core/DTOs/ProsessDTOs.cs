using ProsessPortal.Core.Entities;

namespace ProsessPortal.Core.DTOs;

public record ProsessListDto(
    int Id,
    string Title,
    string Description,
    string Category,
    ProsessStatus Status,
    DateTime UpdatedAt,
    string CreatedByUserName,
    string? OwnerName,
    int ViewCount,
    ICollection<string> Tags
);

public record ProsessDetailDto(
    int Id,
    string Title,
    string Description,
    string Category,
    ProsessStatus Status,
    string? GitRepository,
    string? GitPath,
    string? GitBranch,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string CreatedByUserName,
    string? OwnerName,
    int ViewCount,
    DateTime? LastAccessedAt,
    ICollection<ProsessTagDto> Tags,
    ICollection<ProsessStepDto> Steps,
    ProsessVersionDto? CurrentVersion,
    ICollection<ProsessVersionSummaryDto> VersionHistory
);

public record ProsessStepDto(
    int Id,
    string Title,
    string Description,
    string? DetailedInstructions,
    int OrderIndex,
    StepType Type,
    string? ResponsibleRole,
    int? EstimatedDurationMinutes,
    bool IsOptional,
    int? ParentStepId,
    ICollection<ProsessStepDto> SubSteps,
    ICollection<StepConnectionDto> OutgoingConnections
);

public record StepConnectionDto(
    int Id,
    int ToStepId,
    string? Condition,
    ConnectionType Type
);

public record ProsessTagDto(
    int Id,
    string Name,
    string Color
);

public record ProsessVersionDto(
    int Id,
    string VersionNumber,
    string Title,
    string Description,
    string Content,
    string ChangeLog,
    DateTime CreatedAt,
    string CreatedByUserName,
    bool IsCurrent,
    bool IsPublished,
    DateTime? PublishedAt,
    string? PublishedByUserName
);

public record ProsessVersionSummaryDto(
    int Id,
    string VersionNumber,
    string Title,
    DateTime CreatedAt,
    string CreatedByUserName,
    bool IsCurrent,
    bool IsPublished
);

public record CreateProsessRequest(
    string Title,
    string Description,
    string Category,
    string? ITILArea = null,
    string? Priority = null,
    int? OwnerId = null,
    ICollection<string>? Tags = null
);

public record UpdateProsessRequest(
    string Title,
    string Description,
    string Category,
    string? ITILArea = null,
    string? Priority = null,
    int? OwnerId = null,
    ICollection<string>? Tags = null
);

public record CreateProsessStepRequest(
    string Title,
    string Description,
    string? DetailedInstructions,
    int OrderIndex,
    StepType Type,
    string? ResponsibleRole,
    int? EstimatedDurationMinutes,
    bool IsOptional,
    int? ParentStepId
);

public record ProsessSearchRequest(
    string? Search = null,
    string? Category = null,
    ProsessStatus? Status = null,
    string? Tag = null,
    string? CreatedBy = null,
    string? Owner = null,
    DateTime? CreatedAfter = null,
    DateTime? CreatedBefore = null,
    int Page = 1,
    int PageSize = 20,
    string SortBy = "UpdatedAt",
    bool SortDescending = true
);

public record PagedResult<T>(
    ICollection<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

public record ProsessStatisticsDto(
    int TotalProsesses,
    int PublishedProsesses,
    int DraftProsesses,
    int InReviewProsesses,
    Dictionary<string, int> ProcessesByCategory,
    Dictionary<string, int> ProcessesByStatus,
    int TotalViews,
    ProsessListDto? MostViewedProsess,
    ProsessListDto? RecentlyUpdatedProsess
);

// ITIL-specific DTOs for Epic 4
public record ITILAreaDto(
    string Name,
    string Description,
    ICollection<string> CommonProcesses
);

public record ITILProcessTemplateDto(
    string Name,
    string ITILArea,
    string Purpose,
    string Description,
    ICollection<string> KeyActivities,
    ICollection<string> Inputs,
    ICollection<string> Outputs,
    ICollection<string> KPIs,
    ICollection<CreateProsessStepRequest> DefaultSteps
);

public record ProsessCategoriesDto(
    ICollection<string> BusinessCategories,
    ICollection<ITILAreaDto> ITILAreas,
    ICollection<string> Priorities
);