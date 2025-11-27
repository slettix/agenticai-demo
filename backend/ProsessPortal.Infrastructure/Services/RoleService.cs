using Microsoft.EntityFrameworkCore;
using ProsessPortal.Core.DTOs;
using ProsessPortal.Core.Entities;
using ProsessPortal.Infrastructure.Data;

namespace ProsessPortal.Infrastructure.Services;

public interface IRoleService
{
    Task<RoleListDTO> GetRolesAsync(RoleSearchDTO search);
    Task<RoleDTO?> GetRoleByIdAsync(int id);
    Task<RoleDTO> CreateRoleAsync(CreateRoleDTO createRole, int currentUserId);
    Task<RoleDTO> UpdateRoleAsync(int id, UpdateRoleDTO updateRole, int currentUserId);
    Task<bool> DeleteRoleAsync(int id);
    Task<bool> ActivateRoleAsync(int id, int currentUserId);
    Task<bool> DeactivateRoleAsync(int id, int currentUserId);
    Task<List<PermissionDTO>> GetAllPermissionsAsync();
    Task<List<PermissionDTO>> GetRolePermissionsAsync(int roleId);
    Task<bool> AssignPermissionToRoleAsync(int roleId, int permissionId);
    Task<bool> RemovePermissionFromRoleAsync(int roleId, int permissionId);
    Task<RoleCategoriesDTO> GetRoleCategoriesAsync();
}

public class RoleService : IRoleService
{
    private readonly ProsessPortalDbContext _context;

    public RoleService(ProsessPortalDbContext context)
    {
        _context = context;
    }

    public async Task<RoleListDTO> GetRolesAsync(RoleSearchDTO search)
    {
        var query = _context.Roles
            .Include(r => r.CreatedByUser)
            .Include(r => r.UpdatedByUser)
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search.SearchTerm))
        {
            query = query.Where(r => r.Name.Contains(search.SearchTerm) || 
                                    r.Description.Contains(search.SearchTerm));
        }

        if (search.Category.HasValue)
        {
            query = query.Where(r => r.Category == search.Category.Value);
        }

        if (search.Level.HasValue)
        {
            query = query.Where(r => r.Level == search.Level.Value);
        }

        if (search.IsActive.HasValue)
        {
            query = query.Where(r => r.IsActive == search.IsActive.Value);
        }

        var totalCount = await query.CountAsync();

        var roles = await query
            .OrderBy(r => r.Name)
            .Skip((search.Page - 1) * search.PageSize)
            .Take(search.PageSize)
            .ToListAsync();

        return new RoleListDTO
        {
            Roles = roles.Select(MapToDTO).ToList(),
            TotalCount = totalCount,
            Page = search.Page,
            PageSize = search.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / search.PageSize)
        };
    }

    public async Task<RoleDTO?> GetRoleByIdAsync(int id)
    {
        var role = await _context.Roles
            .Include(r => r.CreatedByUser)
            .Include(r => r.UpdatedByUser)
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id);

        return role == null ? null : MapToDTO(role);
    }

    public async Task<RoleDTO> CreateRoleAsync(CreateRoleDTO createRole, int currentUserId)
    {
        var role = new Role
        {
            Name = createRole.Name,
            Description = createRole.Description,
            Category = createRole.Category,
            Level = createRole.Level,
            CreatedByUserId = currentUserId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        // Assign permissions if provided
        if (createRole.PermissionIds?.Any() == true)
        {
            foreach (var permissionId in createRole.PermissionIds)
            {
                var rolePermission = new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permissionId
                };
                _context.RolePermissions.Add(rolePermission);
            }
            await _context.SaveChangesAsync();
        }

        return await GetRoleByIdAsync(role.Id) ?? throw new InvalidOperationException("Failed to create role");
    }

    public async Task<RoleDTO> UpdateRoleAsync(int id, UpdateRoleDTO updateRole, int currentUserId)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null)
            throw new ArgumentException("Role not found");

        role.Name = updateRole.Name;
        role.Description = updateRole.Description;
        role.Category = updateRole.Category;
        role.Level = updateRole.Level;
        role.IsActive = updateRole.IsActive;
        role.UpdatedByUserId = currentUserId;
        role.UpdatedAt = DateTime.UtcNow;

        // Update permissions if provided
        if (updateRole.PermissionIds != null)
        {
            // Remove existing permissions
            var existingPermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == id)
                .ToListAsync();
            _context.RolePermissions.RemoveRange(existingPermissions);

            // Add new permissions
            foreach (var permissionId in updateRole.PermissionIds)
            {
                var rolePermission = new RolePermission
                {
                    RoleId = id,
                    PermissionId = permissionId
                };
                _context.RolePermissions.Add(rolePermission);
            }
        }

        await _context.SaveChangesAsync();
        return await GetRoleByIdAsync(id) ?? throw new InvalidOperationException("Failed to update role");
    }

    public async Task<bool> DeleteRoleAsync(int id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null) return false;

        // Check if role is assigned to any users or actors
        var hasUsers = await _context.UserRoles.AnyAsync(ur => ur.RoleId == id);
        var hasActors = await _context.ActorRoles.AnyAsync(ar => ar.RoleId == id);

        if (hasUsers || hasActors)
        {
            // Don't delete, just deactivate to preserve historical data
            role.IsActive = false;
            role.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            // Remove role permissions first
            var rolePermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == id)
                .ToListAsync();
            _context.RolePermissions.RemoveRange(rolePermissions);
            
            // Remove role
            _context.Roles.Remove(role);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActivateRoleAsync(int id, int currentUserId)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null) return false;

        role.IsActive = true;
        role.UpdatedByUserId = currentUserId;
        role.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeactivateRoleAsync(int id, int currentUserId)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null) return false;

        role.IsActive = false;
        role.UpdatedByUserId = currentUserId;
        role.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<PermissionDTO>> GetAllPermissionsAsync()
    {
        var permissions = await _context.Permissions
            .OrderBy(p => p.Resource)
            .ThenBy(p => p.Action)
            .ToListAsync();

        return permissions.Select(p => new PermissionDTO
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Resource = p.Resource,
            Action = p.Action
        }).ToList();
    }

    public async Task<List<PermissionDTO>> GetRolePermissionsAsync(int roleId)
    {
        var permissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission)
            .OrderBy(p => p.Resource)
            .ThenBy(p => p.Action)
            .ToListAsync();

        return permissions.Select(p => new PermissionDTO
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Resource = p.Resource,
            Action = p.Action
        }).ToList();
    }

    public async Task<bool> AssignPermissionToRoleAsync(int roleId, int permissionId)
    {
        // Check if permission is already assigned
        var existingAssignment = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

        if (existingAssignment != null) return true; // Already assigned

        var rolePermission = new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId
        };

        _context.RolePermissions.Add(rolePermission);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemovePermissionFromRoleAsync(int roleId, int permissionId)
    {
        var rolePermission = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

        if (rolePermission == null) return false;

        _context.RolePermissions.Remove(rolePermission);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<RoleCategoriesDTO> GetRoleCategoriesAsync()
    {
        return new RoleCategoriesDTO
        {
            Categories = new Dictionary<int, string>
            {
                { (int)RoleCategory.Internal, "Intern" },
                { (int)RoleCategory.External, "Ekstern" },
                { (int)RoleCategory.System, "System" }
            },
            Levels = new Dictionary<int, string>
            {
                { (int)OrganizationLevel.Individual, "Individuell" },
                { (int)OrganizationLevel.Unit, "Enhet" },
                { (int)OrganizationLevel.Department, "Avdeling" },
                { (int)OrganizationLevel.Organization, "Organisasjon" },
                { (int)OrganizationLevel.National, "Nasjonalt" }
            }
        };
    }

    private RoleDTO MapToDTO(Role role)
    {
        return new RoleDTO
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            Category = role.Category,
            Level = role.Level,
            IsActive = role.IsActive,
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt,
            CreatedByUserName = role.CreatedByUser != null ? $"{role.CreatedByUser.FirstName} {role.CreatedByUser.LastName}" : "",
            UpdatedByUserName = role.UpdatedByUser != null ? $"{role.UpdatedByUser.FirstName} {role.UpdatedByUser.LastName}" : null,
            Permissions = role.RolePermissions?.Select(rp => new PermissionDTO
            {
                Id = rp.Permission.Id,
                Name = rp.Permission.Name,
                Description = rp.Permission.Description,
                Resource = rp.Permission.Resource,
                Action = rp.Permission.Action
            }).ToList()
        };
    }
}