namespace ProsessPortal.Core.Entities;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public static class RoleNames
{
    public const string Admin = "Admin";
    public const string ProsessEier = "ProsessEier";
    public const string QA = "QA";
    public const string SME = "SME";
    public const string Bruker = "Bruker";
}