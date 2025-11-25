using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProsessPortal.Core.DTOs;
using ProsessPortal.Core.Interfaces;
using System.Security.Claims;

namespace ProsessPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DeletionController : ControllerBase
{
    private readonly IDeletionService _deletionService;
    private readonly ILogger<DeletionController> _logger;

    public DeletionController(IDeletionService deletionService, ILogger<DeletionController> logger)
    {
        _deletionService = deletionService;
        _logger = logger;
    }

    [HttpPost("{prosessId}/soft-delete")]
    public async Task<ActionResult> SoftDeleteProcess(int prosessId, [FromBody] DeleteProsessRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var success = await _deletionService.SoftDeleteProcessAsync(prosessId, userId.Value, request);
            if (!success) return NotFound("Prosess ikke funnet");

            _logger.LogInformation("Process soft deleted: {ProcessId} by user {UserId}", prosessId, userId);
            return Ok(new { message = "Prosessen ble slettet", prosessId });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbidden(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error soft deleting process {ProcessId}", prosessId);
            return StatusCode(500, "En feil oppstod under sletting av prosess");
        }
    }

    [HttpPost("{prosessId}/hard-delete")]
    [Authorize(Policy = "RequireDeleteProsess")]
    public async Task<ActionResult> HardDeleteProcess(int prosessId, [FromBody] DeleteProsessRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var success = await _deletionService.HardDeleteProcessAsync(prosessId, userId.Value, request);
            if (!success) return NotFound("Prosess ikke funnet");

            _logger.LogInformation("Process hard deleted: {ProcessId} by user {UserId}", prosessId, userId);
            return Ok(new { message = "Prosessen ble permanent slettet", prosessId });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbidden(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error hard deleting process {ProcessId}", prosessId);
            return StatusCode(500, "En feil oppstod under permanent sletting av prosess");
        }
    }

    [HttpPost("{prosessId}/restore")]
    public async Task<ActionResult> RestoreProcess(int prosessId, [FromBody] RestoreProsessRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var success = await _deletionService.RestoreProcessAsync(prosessId, userId.Value, request);
            if (!success) return NotFound("Slettet prosess ikke funnet");

            _logger.LogInformation("Process restored: {ProcessId} by user {UserId}", prosessId, userId);
            return Ok(new { message = "Prosessen ble gjenopprettet", prosessId });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbidden(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring process {ProcessId}", prosessId);
            return StatusCode(500, "En feil oppstod under gjenoppretting av prosess");
        }
    }

    [HttpGet("deleted")]
    public async Task<ActionResult<PagedResult<DeletedProsessDto>>> GetDeletedProcesses([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var result = await _deletionService.GetDeletedProcessesAsync(userId.Value, page, pageSize);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbidden(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting deleted processes");
            return StatusCode(500, "En feil oppstod under henting av slettede prosesser");
        }
    }

    [HttpPost("bulk-delete")]
    [Authorize(Policy = "RequireDeleteProsess")]
    public async Task<ActionResult<BulkDeleteResult>> BulkDeleteProcesses([FromBody] BulkDeleteRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var result = await _deletionService.BulkDeleteProcessesAsync(request.ProsessIds, userId.Value, request);
            _logger.LogInformation("Bulk delete completed: {Successful}/{Total} by user {UserId}", 
                result.SuccessfullyDeleted, result.TotalRequested, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk deleting processes");
            return StatusCode(500, "En feil oppstod under bulk-sletting av prosesser");
        }
    }

    [HttpGet("{prosessId}/deletion-history")]
    public async Task<ActionResult<ICollection<DeletionHistoryDto>>> GetDeletionHistory(int prosessId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            // Check if user can view deletion history for this process
            if (!await _deletionService.CanUserDeleteProcessAsync(userId.Value, prosessId))
                return Forbidden("Du har ikke tilgang til slettingshistorikken for denne prosessen");

            var history = await _deletionService.GetDeletionHistoryAsync(prosessId);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting deletion history for process {ProcessId}", prosessId);
            return StatusCode(500, "En feil oppstod under henting av slettingshistorikk");
        }
    }

    [HttpGet("{prosessId}/has-active-instances")]
    public async Task<ActionResult<bool>> HasActiveInstances(int prosessId)
    {
        try
        {
            var hasActive = await _deletionService.HasActiveInstancesAsync(prosessId);
            return Ok(hasActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking active instances for process {ProcessId}", prosessId);
            return StatusCode(500, "En feil oppstod under sjekk av aktive instanser");
        }
    }

    [HttpGet("{prosessId}/can-delete")]
    public async Task<ActionResult<bool>> CanUserDelete(int prosessId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var canDelete = await _deletionService.CanUserDeleteProcessAsync(userId.Value, prosessId);
            return Ok(canDelete);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking delete permissions for process {ProcessId}", prosessId);
            return StatusCode(500, "En feil oppstod under sjekk av slettingstilgang");
        }
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return null;
    }
    
    private ActionResult Forbidden(string message)
    {
        return StatusCode(403, message);
    }
}