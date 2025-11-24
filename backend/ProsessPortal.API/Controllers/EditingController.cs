using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProsessPortal.Core.DTOs;
using ProsessPortal.Core.Interfaces;
using System.Security.Claims;

namespace ProsessPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EditingController : ControllerBase
{
    private readonly IEditingService _editingService;
    private readonly ILogger<EditingController> _logger;

    public EditingController(IEditingService editingService, ILogger<EditingController> logger)
    {
        _editingService = editingService;
        _logger = logger;
    }

    /// <summary>
    /// Start en redigeringssesjon for en prosess
    /// </summary>
    [HttpPost("start/{prosessId}")]
    public async Task<ActionResult<StartEditSessionResponse>> StartEditSession(
        int prosessId, 
        [FromBody] StartEditSessionRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _editingService.StartEditSessionAsync(prosessId, userId, request);
            
            _logger.LogInformation("User {UserId} started editing session for process {ProcessId}", userId, prosessId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting edit session for process {ProcessId}", prosessId);
            return StatusCode(500, new { message = "En feil oppstod ved start av redigeringssesjon" });
        }
    }

    /// <summary>
    /// Avslutt en redigeringssesjon
    /// </summary>
    [HttpPost("end/{sessionId}")]
    public async Task<ActionResult> EndEditSession(string sessionId, [FromBody] string? completionComment = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _editingService.EndEditSessionAsync(sessionId, userId, completionComment);
            
            _logger.LogInformation("User {UserId} ended editing session {SessionId}", userId, sessionId);
            return Ok(new { message = "Redigeringssesjon avsluttet" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending edit session {SessionId}", sessionId);
            return StatusCode(500, new { message = "En feil oppstod ved avslutning av redigeringssesjon" });
        }
    }

    /// <summary>
    /// Lagre endringer som utkast
    /// </summary>
    [HttpPost("draft/{sessionId}")]
    public async Task<ActionResult<ProsessDetailDto>> SaveDraft(
        string sessionId, 
        [FromBody] SaveDraftRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _editingService.SaveDraftAsync(sessionId, request, userId);
            
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving draft for session {SessionId}", sessionId);
            return StatusCode(500, new { message = "En feil oppstod ved lagring av utkast" });
        }
    }

    /// <summary>
    /// Hent utkast for redigeringssesjon
    /// </summary>
    [HttpGet("draft/{sessionId}")]
    public async Task<ActionResult<ProsessDetailDto>> GetDraft(string sessionId)
    {
        try
        {
            var result = await _editingService.GetDraftAsync(sessionId);
            
            if (result == null)
                return NotFound(new { message = "Ingen utkast funnet for denne sesjonen" });
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting draft for session {SessionId}", sessionId);
            return StatusCode(500, new { message = "En feil oppstod ved henting av utkast" });
        }
    }

    /// <summary>
    /// Fullfør redigering og opprett ny versjon
    /// </summary>
    [HttpPost("complete/{sessionId}")]
    public async Task<ActionResult<ProsessDetailDto>> CompleteEdit(
        string sessionId, 
        [FromBody] EditProsessRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _editingService.CompleteEditWithNewVersionAsync(sessionId, request, userId);
            
            _logger.LogInformation("User {UserId} completed editing session {SessionId}", userId, sessionId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing edit session {SessionId}", sessionId);
            return StatusCode(500, new { message = "En feil oppstod ved fullføring av redigering" });
        }
    }

    /// <summary>
    /// Hent aktive redigeringssesjoner for en prosess
    /// </summary>
    [HttpGet("sessions/{prosessId}")]
    public async Task<ActionResult<ICollection<ProsessEditSessionDto>>> GetActiveEditSessions(int prosessId)
    {
        try
        {
            var result = await _editingService.GetActiveEditSessionsAsync(prosessId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active edit sessions for process {ProcessId}", prosessId);
            return StatusCode(500, new { message = "En feil oppstod ved henting av aktive redigeringssesjoner" });
        }
    }

    /// <summary>
    /// Sammenlign to versjoner av en prosess
    /// </summary>
    [HttpPost("compare")]
    public async Task<ActionResult<ProsessDiffDto>> CompareVersions([FromBody] ProsessVersionCompareRequest request)
    {
        try
        {
            var result = await _editingService.CompareProsessVersionsAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing versions {FromVersionId} and {ToVersionId}", 
                request.FromVersionId, request.ToVersionId);
            return StatusCode(500, new { message = "En feil oppstod ved sammenligning av versjoner" });
        }
    }

    /// <summary>
    /// Sammenlign utkast med en versjon
    /// </summary>
    [HttpPost("compare-draft/{sessionId}/{versionId}")]
    public async Task<ActionResult<ProsessDiffDto>> CompareWithDraft(string sessionId, int versionId)
    {
        try
        {
            var result = await _editingService.CompareWithDraftAsync(sessionId, versionId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing draft in session {SessionId} with version {VersionId}", 
                sessionId, versionId);
            return StatusCode(500, new { message = "En feil oppstod ved sammenligning med utkast" });
        }
    }

    /// <summary>
    /// Sjekk om bruker kan redigere prosess
    /// </summary>
    [HttpGet("can-edit/{prosessId}")]
    public async Task<ActionResult<bool>> CanUserEditProcess(int prosessId)
    {
        var userId = GetCurrentUserId();
        var result = await _editingService.CanUserEditProcessAsync(prosessId, userId);
        return Ok(result);
    }

    /// <summary>
    /// Auto-lagre endringer
    /// </summary>
    [HttpPost("autosave/{sessionId}")]
    public async Task<ActionResult<bool>> AutoSave(string sessionId, [FromBody] SaveDraftRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _editingService.AutoSaveAsync(sessionId, request, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error auto-saving for session {SessionId}", sessionId);
            return StatusCode(500, new { message = "En feil oppstod ved auto-lagring" });
        }
    }

    /// <summary>
    /// Hent redigeringsstatistikk
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<EditingStatisticsDto>> GetEditingStatistics()
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _editingService.GetEditingStatisticsAsync(userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting editing statistics for user {UserId}", GetCurrentUserId());
            return StatusCode(500, new { message = "En feil oppstod ved henting av redigeringsstatistikk" });
        }
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            throw new UnauthorizedAccessException("Bruker ikke identifisert");
        }
        return userId;
    }
}