namespace ProsessPortal.Core.Entities;

public class ProsessStep
{
    public int Id { get; set; }
    public int ProsessId { get; set; }
    public Prosess Prosess { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DetailedInstructions { get; set; }
    public int OrderIndex { get; set; } // For ordering steps
    public StepType Type { get; set; } = StepType.Action;
    public string? ResponsibleRole { get; set; }
    public int? EstimatedDurationMinutes { get; set; }
    public bool IsOptional { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Parent-child relationship for sub-steps
    public int? ParentStepId { get; set; }
    public ProsessStep? ParentStep { get; set; }
    public ICollection<ProsessStep> SubSteps { get; set; } = new List<ProsessStep>();
    
    // Workflow connections
    public ICollection<StepConnection> OutgoingConnections { get; set; } = new List<StepConnection>();
    public ICollection<StepConnection> IncomingConnections { get; set; } = new List<StepConnection>();
}

public class StepConnection
{
    public int Id { get; set; }
    public int FromStepId { get; set; }
    public ProsessStep FromStep { get; set; } = null!;
    public int ToStepId { get; set; }
    public ProsessStep ToStep { get; set; } = null!;
    public string? Condition { get; set; } // e.g., "if approved", "if rejected"
    public ConnectionType Type { get; set; } = ConnectionType.Sequential;
}

public enum StepType
{
    Start = 0,
    Action = 1,
    Decision = 2,
    Approval = 3,
    Review = 4,
    Wait = 5,
    End = 6,
    Subprocess = 7
}

public enum ConnectionType
{
    Sequential = 0,   // Normal next step
    Conditional = 1,  // Based on condition/decision
    Parallel = 2,     // Multiple paths can execute simultaneously
    Loop = 3,         // Return to previous step
    Exception = 4     // Error handling path
}