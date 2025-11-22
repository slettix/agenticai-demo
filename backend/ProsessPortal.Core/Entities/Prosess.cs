namespace ProsessPortal.Core.Entities;

public class Prosess
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public ProsessStatus Status { get; set; } = ProsessStatus.Draft;
    public string? GitRepository { get; set; }
    public string? GitPath { get; set; }
    public string? GitBranch { get; set; } = "main";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;
    public int? OwnerId { get; set; }
    public User? Owner { get; set; }
    public bool IsActive { get; set; } = true;
    public int ViewCount { get; set; } = 0;
    public DateTime? LastAccessedAt { get; set; }
    
    // Navigation properties
    public ICollection<ProsessVersion> Versions { get; set; } = new List<ProsessVersion>();
    public ICollection<ProsessStep> Steps { get; set; } = new List<ProsessStep>();
    public ICollection<ProsessTag> Tags { get; set; } = new List<ProsessTag>();
}

public enum ProsessStatus
{
    Draft = 0,
    InReview = 1,
    Approved = 2,
    Published = 3,
    Deprecated = 4,
    Archived = 5
}