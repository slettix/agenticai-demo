using System.ComponentModel.DataAnnotations;

namespace ProsessPortal.Core.DTOs;

public record ProcessGenerationRequest(
    [Required] string Title,
    [Required] string Description,
    [Required] string Category,
    ICollection<string>? Requirements = null,
    string? TargetAudience = null,
    string ComplexityLevel = "medium"
);

public record ProcessRevisionRequest(
    [Required] int ProcessId,
    [Required] string RevisionType,
    ICollection<string>? Feedback = null,
    ICollection<string>? ImprovementGoals = null,
    string? CustomInstructions = null
);

public record AgentJobResponse(
    string JobId,
    string Status,
    string Message,
    int? EstimatedDuration = null
);

public record AgentJobStatusResponse(
    string JobId,
    string Status,
    int? Progress,
    string? Message,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    string? ErrorMessage
);

public record ProcessGenerationResult(
    string Title,
    string Description,
    string Category,
    ICollection<GeneratedStepDto> Steps,
    int? EstimatedDuration,
    ICollection<string>? Tags,
    Dictionary<string, object>? Metadata
);

public record ProcessRevisionResult(
    int ProcessId,
    string RevisionSummary,
    string? UpdatedTitle,
    string? UpdatedDescription,
    ICollection<GeneratedStepDto>? UpdatedSteps,
    ICollection<string> ChangesMade,
    Dictionary<string, object>? ImprovementMetrics,
    Dictionary<string, object>? Metadata
);

public record GeneratedStepDto(
    string Title,
    string Description,
    string Type,
    string? ResponsibleRole,
    int? EstimatedDuration,
    int OrderIndex,
    bool IsOptional = false,
    string? DetailedInstructions = null
);