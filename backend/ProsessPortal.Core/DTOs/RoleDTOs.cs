using ProsessPortal.Core.Entities;

namespace ProsessPortal.Core.DTOs;

public class RoleDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RoleCategory Category { get; set; }
    public OrganizationLevel Level { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public string? UpdatedByUserName { get; set; }
    
    public List<PermissionDTO>? Permissions { get; set; }
}

public class CreateRoleDTO
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RoleCategory Category { get; set; } = RoleCategory.Internal;
    public OrganizationLevel Level { get; set; } = OrganizationLevel.Individual;
    public List<int>? PermissionIds { get; set; }
}

public class UpdateRoleDTO
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RoleCategory Category { get; set; }
    public OrganizationLevel Level { get; set; }
    public bool IsActive { get; set; }
    public List<int>? PermissionIds { get; set; }
}

public class RoleSearchDTO
{
    public string? SearchTerm { get; set; }
    public RoleCategory? Category { get; set; }
    public OrganizationLevel? Level { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class RoleListDTO
{
    public List<RoleDTO> Roles { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class PermissionDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
}

public class AssignPermissionDTO
{
    public int PermissionId { get; set; }
}

public class RoleCategoriesDTO
{
    public Dictionary<int, string> Categories { get; set; } = new();
    public Dictionary<int, string> Levels { get; set; } = new();
}