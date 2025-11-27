namespace ProsessPortal.Core.Entities;

public enum ActorCategory
{
    Person,           // Individuell person
    Organization,     // Organisasjon (bedrift, leverandør)
    Unit             // Militær enhet (avdeling, brigade, kompani)
}

public enum ActorType
{
    Internal,           // Forsvaret ansatte/enheter
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
    public ActorCategory ActorCategory { get; set; } = ActorCategory.Person;
    
    // Personal information (for Person category)
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    
    // Organization/Unit information  
    public string? OrganizationName { get; set; }
    public string? UnitName { get; set; }
    public string? UnitType { get; set; } // Brigade, Battalion, Kompani, etc.
    public string? UnitCode { get; set; } // Military unit identifier
    
    // Common contact information
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public ActorType ActorType { get; set; }
    public SecurityClearance SecurityClearance { get; set; } = SecurityClearance.None;
    
    // Additional organizational information
    public string? Department { get; set; }
    public string? Position { get; set; }
    public string? ManagerName { get; set; }
    public string? ManagerEmail { get; set; }
    
    // Organization-specific fields
    public string? RegistrationNumber { get; set; } // Org number for companies
    public string? ParentOrganization { get; set; } // Parent company/organization
    public int? EmployeeCount { get; set; }
    
    // Unit-specific fields  
    public string? CommandStructure { get; set; } // Which command the unit reports to
    public string? UnitMission { get; set; } // Unit's primary mission
    public int? PersonnelCount { get; set; } // Number of personnel in unit
    
    // Location and contact
    public string? GeographicLocation { get; set; }
    public string? Address { get; set; }
    public string? PreferredLanguage { get; set; } = "NO";
    
    
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
    
    // Computed properties
    public string DisplayName => ActorCategory switch
    {
        ActorCategory.Person => $"{FirstName} {LastName}".Trim(),
        ActorCategory.Organization => OrganizationName ?? "Ukjent organisasjon",
        ActorCategory.Unit => UnitName ?? "Ukjent enhet",
        _ => "Ukjent aktør"
    };
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