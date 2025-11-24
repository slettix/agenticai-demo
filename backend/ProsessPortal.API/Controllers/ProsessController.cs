using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProsessPortal.Core.DTOs;
using ProsessPortal.Core.Interfaces;
using System.Security.Claims;

namespace ProsessPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProsessController : ControllerBase
{
    private readonly IProsessService _prosessService;
    private readonly ILogger<ProsessController> _logger;

    public ProsessController(IProsessService prosessService, ILogger<ProsessController> logger)
    {
        _prosessService = prosessService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProsessListDto>>> SearchProsesses([FromQuery] ProsessSearchRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _prosessService.SearchProsessesAsync(request, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching processes");
            return StatusCode(500, "En feil oppstod under søk i prosesser");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProsessDetailDto>> GetProsess(int id)
    {
        try
        {
            var prosess = await _prosessService.GetProsessDetailAsync(id);
            if (prosess == null)
            {
                return NotFound("Prosess ikke funnet");
            }

            // Record view
            await _prosessService.RecordProsessViewAsync(id);

            return Ok(prosess);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting process {ProcessId}", id);
            return StatusCode(500, "En feil oppstod under henting av prosess");
        }
    }

    [HttpPost]
    [Authorize(Policy = "RequireCreateProsess")]
    public async Task<ActionResult<ProsessDetailDto>> CreateProsess([FromBody] CreateProsessRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Category))
            {
                return BadRequest("Tittel og kategori er påkrevd");
            }

            var prosess = await _prosessService.CreateProsessAsync(request, userId.Value);
            if (prosess == null)
            {
                return BadRequest("Kunne ikke opprette prosess");
            }

            _logger.LogInformation("Process created: {ProcessTitle} by user {UserId}", request.Title, userId);
            return CreatedAtAction(nameof(GetProsess), new { id = prosess.Id }, prosess);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating process");
            return StatusCode(500, "En feil oppstod under opprettelse av prosess");
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "RequireEditProsess")]
    public async Task<ActionResult<ProsessDetailDto>> UpdateProsess(int id, [FromBody] UpdateProsessRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Category))
            {
                return BadRequest("Tittel og kategori er påkrevd");
            }

            var prosess = await _prosessService.UpdateProsessAsync(id, request, userId.Value);
            if (prosess == null)
            {
                return NotFound("Prosess ikke funnet");
            }

            _logger.LogInformation("Process updated: {ProcessId} by user {UserId}", id, userId);
            return Ok(prosess);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating process {ProcessId}", id);
            return StatusCode(500, "En feil oppstod under oppdatering av prosess");
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "RequireDeleteProsess")]
    public async Task<ActionResult> DeleteProsess(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var success = await _prosessService.DeleteProsessAsync(id, userId.Value);
            if (!success)
            {
                return NotFound("Prosess ikke funnet");
            }

            _logger.LogInformation("Process deleted: {ProcessId} by user {UserId}", id, userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting process {ProcessId}", id);
            return StatusCode(500, "En feil oppstod under sletting av prosess");
        }
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<ProsessStatisticsDto>> GetStatistics()
    {
        try
        {
            var statistics = await _prosessService.GetProsessStatisticsAsync();
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting process statistics");
            return StatusCode(500, "En feil oppstod under henting av statistikk");
        }
    }

    [HttpGet("categories")]
    public async Task<ActionResult<ProsessCategoriesDto>> GetCategories()
    {
        try
        {
            var categories = await _prosessService.GetCategoriesWithITILAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return StatusCode(500, "En feil oppstod under henting av kategorier");
        }
    }

    [HttpGet("itil/areas")]
    public async Task<ActionResult<ICollection<ITILAreaDto>>> GetITILAreas()
    {
        try
        {
            var areas = await _prosessService.GetITILAreasAsync();
            return Ok(areas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ITIL areas");
            return StatusCode(500, "En feil oppstod under henting av ITIL-områder");
        }
    }

    [HttpGet("itil/templates")]
    public async Task<ActionResult<ICollection<ITILProcessTemplateDto>>> GetITILTemplates([FromQuery] string? area = null)
    {
        try
        {
            var templates = await _prosessService.GetITILTemplatesAsync(area);
            return Ok(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ITIL templates");
            return StatusCode(500, "En feil oppstod under henting av ITIL-maler");
        }
    }

    [HttpGet("tags")]
    public async Task<ActionResult<ICollection<ProsessTagDto>>> GetTags()
    {
        try
        {
            var tags = await _prosessService.GetTagsAsync();
            return Ok(tags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tags");
            return StatusCode(500, "En feil oppstod under henting av tags");
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
}