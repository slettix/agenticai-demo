using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProsessPortal.Core.DTOs;
using ProsessPortal.Infrastructure.Services;
using System.Security.Claims;

namespace ProsessPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    [HttpGet]
    public async Task<ActionResult<RoleListDTO>> GetRoles([FromQuery] RoleSearchDTO search)
    {
        try
        {
            var result = await _roleService.GetRolesAsync(search);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoleDTO>> GetRole(int id)
    {
        try
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
                return NotFound(new { error = "Role not found" });

            return Ok(role);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Policy = "RequireManageRoles")]
    public async Task<ActionResult<RoleDTO>> CreateRole([FromBody] CreateRoleDTO createRole)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var role = await _roleService.CreateRoleAsync(createRole, currentUserId);
            return CreatedAtAction(nameof(GetRole), new { id = role.Id }, role);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "RequireManageRoles")]
    public async Task<ActionResult<RoleDTO>> UpdateRole(int id, [FromBody] UpdateRoleDTO updateRole)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var role = await _roleService.UpdateRoleAsync(id, updateRole, currentUserId);
            return Ok(role);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "RequireManageRoles")]
    public async Task<ActionResult> DeleteRole(int id)
    {
        try
        {
            var result = await _roleService.DeleteRoleAsync(id);
            if (!result)
                return NotFound(new { error = "Role not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/activate")]
    [Authorize(Policy = "RequireManageRoles")]
    public async Task<ActionResult> ActivateRole(int id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var result = await _roleService.ActivateRoleAsync(id, currentUserId);
            if (!result)
                return NotFound(new { error = "Role not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/deactivate")]
    [Authorize(Policy = "RequireManageRoles")]
    public async Task<ActionResult> DeactivateRole(int id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var result = await _roleService.DeactivateRoleAsync(id, currentUserId);
            if (!result)
                return NotFound(new { error = "Role not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("permissions")]
    public async Task<ActionResult<List<PermissionDTO>>> GetAllPermissions()
    {
        try
        {
            var permissions = await _roleService.GetAllPermissionsAsync();
            return Ok(permissions);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}/permissions")]
    public async Task<ActionResult<List<PermissionDTO>>> GetRolePermissions(int id)
    {
        try
        {
            var permissions = await _roleService.GetRolePermissionsAsync(id);
            return Ok(permissions);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{roleId}/permissions/{permissionId}")]
    [Authorize(Policy = "RequireManageRoles")]
    public async Task<ActionResult> AssignPermissionToRole(int roleId, int permissionId)
    {
        try
        {
            var result = await _roleService.AssignPermissionToRoleAsync(roleId, permissionId);
            if (!result)
                return BadRequest(new { error = "Failed to assign permission" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{roleId}/permissions/{permissionId}")]
    [Authorize(Policy = "RequireManageRoles")]
    public async Task<ActionResult> RemovePermissionFromRole(int roleId, int permissionId)
    {
        try
        {
            var result = await _roleService.RemovePermissionFromRoleAsync(roleId, permissionId);
            if (!result)
                return NotFound(new { error = "Permission assignment not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("categories")]
    public async Task<ActionResult<RoleCategoriesDTO>> GetRoleCategories()
    {
        try
        {
            var categories = await _roleService.GetRoleCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}