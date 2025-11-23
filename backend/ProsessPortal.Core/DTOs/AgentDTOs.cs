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

// Epic 3: AI-driven Process Automation DTOs

public record DocumentClassificationRequest(
    [Required] string FileName,
    [Required] string FileType,
    string? FileContent = null,
    int FileSize = 0
);

public record DocumentClassificationResult(
    DocumentInfoDto DocumentInfo,
    ClassificationDto Classification,
    ProcessRecommendationDto ProcessRecommendation,
    Dictionary<string, object>? Metadata
);

public record DocumentInfoDto(
    string FileName,
    string FileType,
    int FileSize
);

public record ClassificationDto(
    string Category,
    string CategoryName,
    double Confidence,
    string Description,
    ICollection<string> Keywords
);

public record ProcessRecommendationDto(
    string? SuggestedProcess,
    bool AutoStart,
    bool ManualReviewRequired,
    string? Reasoning
);

public record ProcessOptimizationRequest(
    [Required] int ProcessId,
    string? Title = null,
    string? Description = null,
    ICollection<ProcessStepDto>? Steps = null,
    ProcessHistoricalDataDto? HistoricalData = null
);

public record ProcessOptimizationResult(
    ProcessInfoDto ProcessInfo,
    PerformanceMetricsDto PerformanceMetrics,
    ICollection<BottleneckDto> Bottlenecks,
    TrendsDto Trends,
    ICollection<RecommendationDto> Recommendations,
    int OptimizationScore,
    Dictionary<string, object>? Metadata
);

public record ProcessInfoDto(
    int ProcessId,
    string Title,
    string Description,
    int TotalSteps
);

public record PerformanceMetricsDto(
    int TotalExecutions,
    double AvgDuration,
    double MedianDuration,
    double SuccessRate,
    double AvgStepsCompleted,
    string? MostCommonFailureStep,
    double DurationStdDev
);

public record BottleneckDto(
    int StepId,
    string StepTitle,
    string Severity,
    ICollection<string> Reasons,
    double AvgDuration,
    double FailureRate,
    double EstimatedImprovement
);

public record TrendsDto(
    string TrendDirection,
    double DurationChangePercent,
    double SuccessRateChange,
    string VolumeTrend
);

public record RecommendationDto(
    string Title,
    string Description,
    string Category,
    string Priority,
    double EstimatedTimeSavings,
    double EstimatedSuccessRateImprovement,
    string ImplementationEffort,
    string Impact,
    ICollection<string> SpecificSteps
);

public record ProcessStepDto(
    int? Id,
    string Title,
    string? Description,
    string? Type,
    int? EstimatedDuration
);

public record ProcessHistoricalDataDto(
    ICollection<ProcessExecutionDto>? Executions = null,
    Dictionary<string, StepPerformanceDto>? StepPerformance = null
);

public record ProcessExecutionDto(
    DateTime ExecutedAt,
    double? Duration,
    string? Status,
    int? StepsCompleted,
    string? FailedAtStep
);

public record StepPerformanceDto(
    double? AvgDuration,
    double? FailureRate
);