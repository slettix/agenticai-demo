namespace ProsessPortal.Core.Entities;

public enum ActorType
{
    Internal,           // Forsvaret ansatte
    External,          // Eksterne leverandører/partnere
    Contractor,        // Konsulenter
    Partner,          // Samarbeidspartnere
    Vendor           // Leverandører
}

public enum SecurityClearance
{
    None,
    Restricted,       // Begrenset
    Confidential,     // Konfidensielt
    Secret,          // Hemmelig
    TopSecret        // Strengt hemmelig
}

public class Actor
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public ActorType ActorType { get; set; }
    public SecurityClearance SecurityClearance { get; set; } = SecurityClearance.None;
    
    // Organizational information
    public string? OrganizationName { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public string? ManagerName { get; set; }
    public string? ManagerEmail { get; set; }
    
    // Location and contact
    public string? GeographicLocation { get; set; }
    public string? Address { get; set; }
    public string? PreferredLanguage { get; set; } = "NO";
    
    // Technical competence areas
    public string? CompetenceAreas { get; set; } // JSON array of competence areas
    public string? TechnicalSkills { get; set; } // JSON array of skills
    
    // Contract and access information (for external actors)
    public string? ContractNumber { get; set; }
    public DateTime? ContractStartDate { get; set; }
    public DateTime? ContractEndDate { get; set; }
    public string? VendorId { get; set; } // Reference to vendor organization
    
    // System fields
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;
    public int? UpdatedByUserId { get; set; }
    public User? UpdatedByUser { get; set; }
    
    // Navigation properties
    public ICollection<ActorRole> ActorRoles { get; set; } = new List<ActorRole>();
    public ICollection<ActorNote> Notes { get; set; } = new List<ActorNote>();
}

// Junction table for Actor-Role relationships
public class ActorRole
{
    public int ActorId { get; set; }
    public Actor Actor { get; set; } = null!;
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
    
    // Assignment metadata
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public int AssignedByUserId { get; set; }
    public User AssignedByUser { get; set; } = null!;
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
}

// Notes and comments about actors
public class ActorNote
{
    public int Id { get; set; }
    public int ActorId { get; set; }
    public Actor Actor { get; set; } = null!;
    public string Note { get; set; } = string.Empty;
    public string? Category { get; set; } // e.g., "Security", "Performance", "Contract"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;
    public bool IsPrivate { get; set; } = false; // Only visible to admins
}