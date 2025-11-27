using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProsessPortal.Core.DTOs;
using ProsessPortal.Core.Entities;
using ProsessPortal.Core.Interfaces;
using System.Security.Claims;

namespace ProsessPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ActorController : ControllerBase
{
    private readonly IActorService _actorService;

    public ActorController(IActorService actorService)
    {
        _actorService = actorService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    [HttpGet]
    public async Task<ActionResult<ActorListDTO>> GetActors([FromQuery] ActorSearchDTO search)
    {
        try
        {
            var result = await _actorService.GetActorsAsync(search);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to retrieve actors", details = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ActorDTO>> GetActor(int id)
    {
        try
        {
            var actor = await _actorService.GetActorByIdAsync(id);
            if (actor == null)
                return NotFound(new { error = "Actor not found" });

            return Ok(actor);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to retrieve actor", details = ex.Message });
        }
    }

    [HttpGet("email/{email}")]
    public async Task<ActionResult<ActorDTO>> GetActorByEmail(string email)
    {
        try
        {
            var actor = await _actorService.GetActorByEmailAsync(email);
            if (actor == null)
                return NotFound(new { error = "Actor not found" });

            return Ok(actor);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to retrieve actor", details = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<ActionResult<ActorDTO>> CreateActor([FromBody] CreateActorDTO createActor)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUserId = GetCurrentUserId();
            var actor = await _actorService.CreateActorAsync(createActor, currentUserId);
            
            return CreatedAtAction(nameof(GetActor), new { id = actor.Id }, actor);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to create actor", details = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<ActionResult<ActorDTO>> UpdateActor(int id, [FromBody] UpdateActorDTO updateActor)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUserId = GetCurrentUserId();
            var actor = await _actorService.UpdateActorAsync(id, updateActor, currentUserId);
            
            return Ok(actor);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to update actor", details = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<ActionResult> DeleteActor(int id)
    {
        try
        {
            var success = await _actorService.DeleteActorAsync(id);
            if (!success)
                return NotFound(new { error = "Actor not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to delete actor", details = ex.Message });
        }
    }

    [HttpPost("{id}/activate")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<ActionResult> ActivateActor(int id)
    {
        try
        {
            var success = await _actorService.ActivateActorAsync(id);
            if (!success)
                return NotFound(new { error = "Actor not found" });

            return Ok(new { message = "Actor activated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to activate actor", details = ex.Message });
        }
    }

    [HttpPost("{id}/deactivate")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<ActionResult> DeactivateActor(int id)
    {
        try
        {
            var success = await _actorService.DeactivateActorAsync(id);
            if (!success)
                return NotFound(new { error = "Actor not found" });

            return Ok(new { message = "Actor deactivated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to deactivate actor", details = ex.Message });
        }
    }

    // Role assignment endpoints
    [HttpGet("{id}/roles")]
    public async Task<ActionResult<List<RoleAssignmentDTO>>> GetActorRoles(int id)
    {
        try
        {
            var roles = await _actorService.GetActorRolesAsync(id);
            return Ok(roles);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to retrieve actor roles", details = ex.Message });
        }
    }

    [HttpPost("{id}/roles")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<ActionResult> AssignRoleToActor(int id, [FromBody] AssignRoleToActorDTO assignRole)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUserId = GetCurrentUserId();
            var success = await _actorService.AssignRoleToActorAsync(id, assignRole, currentUserId);
            
            if (!success)
                return NotFound(new { error = "Actor not found" });

            return Ok(new { message = "Role assigned successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to assign role", details = ex.Message });
        }
    }

    [HttpDelete("{id}/roles/{roleId}")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<ActionResult> RemoveRoleFromActor(int id, int roleId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var success = await _actorService.RemoveRoleFromActorAsync(id, roleId, currentUserId);
            
            if (!success)
                return NotFound(new { error = "Actor or role assignment not found" });

            return Ok(new { message = "Role removed successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to remove role", details = ex.Message });
        }
    }

    [HttpPut("{id}/roles/{roleId}")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<ActionResult> UpdateRoleAssignment(int id, int roleId, [FromBody] AssignRoleToActorDTO updateRole)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUserId = GetCurrentUserId();
            var success = await _actorService.UpdateRoleAssignmentAsync(id, roleId, updateRole, currentUserId);
            
            if (!success)
                return NotFound(new { error = "Actor or role assignment not found" });

            return Ok(new { message = "Role assignment updated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to update role assignment", details = ex.Message });
        }
    }

    // Notes endpoints
    [HttpGet("{id}/notes")]
    public async Task<ActionResult<List<ActorNoteDTO>>> GetActorNotes(int id, [FromQuery] bool includePrivate = false)
    {
        try
        {
            // Only admins can see private notes
            if (includePrivate && !User.IsInRole("Admin"))
                includePrivate = false;

            var notes = await _actorService.GetActorNotesAsync(id, includePrivate);
            return Ok(notes);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to retrieve actor notes", details = ex.Message });
        }
    }

    [HttpPost("{id}/notes")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<ActionResult<ActorNoteDTO>> AddActorNote(int id, [FromBody] CreateActorNoteDTO createNote)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUserId = GetCurrentUserId();
            var note = await _actorService.AddActorNoteAsync(id, createNote, currentUserId);
            
            return CreatedAtAction(nameof(GetActorNotes), new { id }, note);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to add note", details = ex.Message });
        }
    }

    [HttpDelete("notes/{noteId}")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<ActionResult> DeleteActorNote(int noteId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var success = await _actorService.DeleteActorNoteAsync(noteId, currentUserId);
            
            if (!success)
                return NotFound(new { error = "Note not found" });

            return Ok(new { message = "Note deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to delete note", details = ex.Message });
        }
    }

    // Search and filter endpoints
    [HttpGet("organizations")]
    public async Task<ActionResult<List<string>>> GetOrganizations()
    {
        try
        {
            var organizations = await _actorService.GetOrganizationsAsync();
            return Ok(organizations);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to retrieve organizations", details = ex.Message });
        }
    }

    [HttpGet("departments")]
    public async Task<ActionResult<List<string>>> GetDepartments()
    {
        try
        {
            var departments = await _actorService.GetDepartmentsAsync();
            return Ok(departments);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to retrieve departments", details = ex.Message });
        }
    }


    // Statistics endpoints
    [HttpGet("stats/actor-types")]
    public async Task<ActionResult<Dictionary<ActorType, int>>> GetActorTypeStats()
    {
        try
        {
            var stats = await _actorService.GetActorTypeStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to retrieve actor type statistics", details = ex.Message });
        }
    }

    [HttpGet("stats/security-clearances")]
    public async Task<ActionResult<Dictionary<SecurityClearance, int>>> GetSecurityClearanceStats()
    {
        try
        {
            var stats = await _actorService.GetSecurityClearanceStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to retrieve security clearance statistics", details = ex.Message });
        }
    }
}