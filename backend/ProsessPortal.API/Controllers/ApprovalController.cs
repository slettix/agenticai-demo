using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProsessPortal.Core.DTOs;
using ProsessPortal.Core.Interfaces;
using System.Security.Claims;

namespace ProsessPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApprovalController : ControllerBase
{
    private readonly IApprovalService _approvalService;
    private readonly ILogger<ApprovalController> _logger;

    public ApprovalController(IApprovalService approvalService, ILogger<ApprovalController> logger)
    {
        _approvalService = approvalService;
        _logger = logger;
    }

    /// <summary>
    /// Send en prosess til godkjenning
    /// </summary>
    [HttpPost("submit/{prosessId}")]
    public async Task<ActionResult<ProsessApprovalRequestDto>> SubmitForApproval(
        int prosessId, 
        [FromBody] SubmitForApprovalRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _approvalService.SubmitForApprovalAsync(prosessId, userId, request);
            
            _logger.LogInformation("User {UserId} submitted process {ProcessId} for approval", userId, prosessId);
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
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Trekk tilbake en godkjenningsforespørsel
    /// </summary>
    [HttpPost("withdraw/{prosessId}")]
    public async Task<ActionResult> WithdrawApprovalRequest(int prosessId)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _approvalService.WithdrawApprovalRequestAsync(prosessId, userId);
            
            _logger.LogInformation("User {UserId} withdrew approval request for process {ProcessId}", userId, prosessId);
            return Ok(new { message = "Godkjenningsforespørsel trukket tilbake" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    /// <summary>
    /// Godkjenn en prosess
    /// </summary>
    [HttpPost("approve/{approvalRequestId}")]
    public async Task<ActionResult<ProsessApprovalRequestDto>> ApproveProcess(
        int approvalRequestId, 
        [FromBody] ApprovalDecisionRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _approvalService.ApproveProcessAsync(approvalRequestId, userId, request);
            
            _logger.LogInformation("User {UserId} approved approval request {RequestId}", userId, approvalRequestId);
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
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Avvis en prosess
    /// </summary>
    [HttpPost("reject/{approvalRequestId}")]
    public async Task<ActionResult<ProsessApprovalRequestDto>> RejectProcess(
        int approvalRequestId, 
        [FromBody] ApprovalDecisionRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _approvalService.RejectProcessAsync(approvalRequestId, userId, request);
            
            _logger.LogInformation("User {UserId} rejected approval request {RequestId}", userId, approvalRequestId);
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
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Legg til kommentar på godkjenningsforespørsel
    /// </summary>
    [HttpPost("comment/{approvalRequestId}")]
    public async Task<ActionResult<ProsessApprovalCommentDto>> AddComment(
        int approvalRequestId, 
        [FromBody] AddApprovalCommentRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _approvalService.AddCommentAsync(approvalRequestId, userId, request);
            
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
    }

    /// <summary>
    /// Hent godkjenningskø
    /// </summary>
    [HttpGet("queue")]
    public async Task<ActionResult<ApprovalQueueDto>> GetApprovalQueue()
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _approvalService.GetApprovalQueueAsync(userId);
            
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    /// <summary>
    /// Hent mine godkjenningsforespørsler
    /// </summary>
    [HttpGet("my-requests")]
    public async Task<ActionResult<MyApprovalRequestsDto>> GetMyApprovalRequests()
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _approvalService.GetMyApprovalRequestsAsync(userId);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            var userId = GetCurrentUserId();
            _logger.LogError(ex, "Error getting approval requests for user {UserId}", userId);
            return StatusCode(500, new { message = "En feil oppstod ved henting av godkjenningsforespørsler" });
        }
    }

    /// <summary>
    /// Hent godkjenningsforespørsel
    /// </summary>
    [HttpGet("request/{approvalRequestId}")]
    public async Task<ActionResult<ProsessApprovalRequestDto>> GetApprovalRequest(int approvalRequestId)
    {
        try
        {
            var result = await _approvalService.GetApprovalRequestAsync(approvalRequestId);
            
            if (result == null)
                return NotFound(new { message = "Godkjenningsforespørsel ikke funnet" });
            
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    /// <summary>
    /// Hent aktiv godkjenningsforespørsel for prosess
    /// </summary>
    [HttpGet("process/{prosessId}/current")]
    public async Task<ActionResult<ProsessApprovalRequestDto>> GetCurrentApprovalRequestForProcess(int prosessId)
    {
        try
        {
            var result = await _approvalService.GetCurrentApprovalRequestForProcessAsync(prosessId);
            
            if (result == null)
                return NotFound(new { message = "Ingen aktiv godkjenningsforespørsel funnet" });
            
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    /// <summary>
    /// Hent godkjenningshistorikk for prosess
    /// </summary>
    [HttpGet("process/{prosessId}/history")]
    public async Task<ActionResult<ICollection<ProsessApprovalHistoryDto>>> GetApprovalHistory(int prosessId)
    {
        try
        {
            var result = await _approvalService.GetApprovalHistoryAsync(prosessId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting approval history for process {ProcessId}", prosessId);
            return StatusCode(500, new { message = "En feil oppstod ved henting av godkjenningshistorikk" });
        }
    }

    /// <summary>
    /// Hent kommentarer for godkjenningsforespørsel
    /// </summary>
    [HttpGet("request/{approvalRequestId}/comments")]
    public async Task<ActionResult<ICollection<ProsessApprovalCommentDto>>> GetComments(int approvalRequestId)
    {
        try
        {
            var result = await _approvalService.GetCommentsAsync(approvalRequestId);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    /// <summary>
    /// Sjekk om bruker kan godkjenne prosess
    /// </summary>
    [HttpGet("can-approve/{prosessId}")]
    public async Task<ActionResult<bool>> CanUserApproveProcess(int prosessId)
    {
        var userId = GetCurrentUserId();
        var result = await _approvalService.CanUserApproveProcessAsync(userId, prosessId);
        return Ok(result);
    }

    /// <summary>
    /// Sjekk om bruker kan sende prosess til godkjenning
    /// </summary>
    [HttpGet("can-submit/{prosessId}")]
    public async Task<ActionResult<bool>> CanUserSubmitForApproval(int prosessId)
    {
        var userId = GetCurrentUserId();
        var result = await _approvalService.CanUserSubmitForApprovalAsync(userId, prosessId);
        return Ok(result);
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