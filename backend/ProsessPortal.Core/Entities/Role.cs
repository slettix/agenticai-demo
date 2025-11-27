namespace ProsessPortal.Core.Entities;

public enum RoleCategory
{
    Internal = 0,    // Interne roller for Forsvaret ansatte
    External = 1,    // Eksterne roller for kontraktører og partnere
    System = 2       // Systemroller
}

public enum OrganizationLevel
{
    Individual = 0,   // Individuell rolle
    Unit = 1,         // Enhets-nivå 
    Department = 2,   // Avdeling-nivå
    Organization = 3, // Organisasjon-nivå
    National = 4      // Nasjonalt nivå
}

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RoleCategory Category { get; set; } = RoleCategory.Internal;
    public OrganizationLevel Level { get; set; } = OrganizationLevel.Individual;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;
    public int? UpdatedByUserId { get; set; }
    public User? UpdatedByUser { get; set; }
    
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<ActorRole> ActorRoles { get; set; } = new List<ActorRole>();
}

public static class RoleNames
{
    public const string Admin = "Admin";
    public const string ProsessEier = "ProsessEier";
    public const string QA = "QA";
    public const string SME = "SME";
    public const string Bruker = "Bruker";
}