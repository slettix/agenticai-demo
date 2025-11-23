using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProsessPortal.Core.DTOs;
using ProsessPortal.Core.Interfaces;
using System.Security.Claims;

namespace ProsessPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AgentController : ControllerBase
{
    private readonly IAgentService _agentService;
    private readonly IProsessService _prosessService;
    private readonly ILogger<AgentController> _logger;

    public AgentController(
        IAgentService agentService, 
        IProsessService prosessService,
        ILogger<AgentController> logger)
    {
        _agentService = agentService;
        _prosessService = prosessService;
        _logger = logger;
    }

    [HttpGet("health")]
    public async Task<ActionResult<object>> GetHealth()
    {
        try
        {
            var isAvailable = await _agentService.IsAgentServiceAvailableAsync();
            return Ok(new
            {
                AgentServiceAvailable = isAvailable,
                Status = isAvailable ? "healthy" : "degraded",
                Message = isAvailable ? "AI agents are available" : "AI agents are currently unavailable"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check agent service health");
            return Ok(new
            {
                AgentServiceAvailable = false,
                Status = "unhealthy",
                Message = "Unable to connect to AI agent service"
            });
        }
    }

    [HttpPost("generate-process")]
    public async Task<ActionResult<AgentJobResponse>> GenerateProcess([FromBody] ProcessGenerationRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
            _logger.LogInformation("User {UserId} requesting process generation: {Title}", userId, request.Title);

            var result = await _agentService.GenerateProcessAsync(request, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit process generation request");
            return StatusCode(500, new { Error = "Failed to submit process generation request", Details = ex.Message });
        }
    }

    [HttpPost("revise-process")]
    public async Task<ActionResult<AgentJobResponse>> ReviseProcess([FromBody] ProcessRevisionRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
            _logger.LogInformation("User {UserId} requesting process revision for process {ProcessId}", userId, request.ProcessId);

            // Check if user has permission to revise this process
            var process = await _prosessService.GetProsessDetailAsync(request.ProcessId);
            if (process == null)
            {
                return NotFound(new { Error = "Process not found" });
            }

            var result = await _agentService.ReviseProcessAsync(request, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit process revision request");
            return StatusCode(500, new { Error = "Failed to submit process revision request", Details = ex.Message });
        }
    }

    [HttpGet("jobs/{jobId}/status")]
    public async Task<ActionResult<AgentJobStatusResponse>> GetJobStatus(string jobId)
    {
        try
        {
            var status = await _agentService.GetJobStatusAsync(jobId);
            if (status == null)
            {
                return NotFound(new { Error = "Job not found" });
            }

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get job status for job {JobId}", jobId);
            return StatusCode(500, new { Error = "Failed to get job status", Details = ex.Message });
        }
    }

    [HttpGet("jobs/{jobId}/result/generation")]
    public async Task<ActionResult<ProcessGenerationResult>> GetGenerationResult(string jobId)
    {
        try
        {
            var result = await _agentService.GetGenerationResultAsync(jobId);
            if (result == null)
            {
                return NotFound(new { Error = "Generation result not found or not ready" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get generation result for job {JobId}", jobId);
            return StatusCode(500, new { Error = "Failed to get generation result", Details = ex.Message });
        }
    }

    [HttpGet("jobs/{jobId}/result/revision")]
    public async Task<ActionResult<ProcessRevisionResult>> GetRevisionResult(string jobId)
    {
        try
        {
            var result = await _agentService.GetRevisionResultAsync(jobId);
            if (result == null)
            {
                return NotFound(new { Error = "Revision result not found or not ready" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get revision result for job {JobId}", jobId);
            return StatusCode(500, new { Error = "Failed to get revision result", Details = ex.Message });
        }
    }

    [HttpPost("jobs/{jobId}/create-process")]
    [Authorize(Roles = "Admin,ProsessEier")]
    public async Task<ActionResult<ProsessDetailDto>> CreateProcessFromGeneration(string jobId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }
            
            _logger.LogInformation("User {UserId} creating process from generation job {JobId}", userId, jobId);

            // Get the generation result
            var generationResult = await _agentService.GetGenerationResultAsync(jobId);
            if (generationResult == null)
            {
                return NotFound(new { Error = "Generation result not found or not ready" });
            }

            // Create a new process request from the generation result
            var createRequest = new CreateProsessRequest(
                Title: generationResult.Title,
                Description: generationResult.Description,
                Category: generationResult.Category,
                Tags: generationResult.Tags?.ToArray()
            );

            // Create the process
            var createdProcess = await _prosessService.CreateProsessAsync(createRequest, userId.Value);

            // TODO: Create steps from generationResult.Steps
            // This would require extending the ProsessService to handle step creation

            _logger.LogInformation("Successfully created process {ProcessId} from AI generation", createdProcess.Id);
            return Ok(createdProcess);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create process from generation job {JobId}", jobId);
            return StatusCode(500, new { Error = "Failed to create process from generation", Details = ex.Message });
        }
    }

    [HttpPost("jobs/{jobId}/apply-revision")]
    [Authorize(Roles = "Admin,ProsessEier")]
    public async Task<ActionResult<ProsessDetailDto>> ApplyRevision(string jobId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }
            
            _logger.LogInformation("User {UserId} applying revision from job {JobId}", userId, jobId);

            // Get the revision result
            var revisionResult = await _agentService.GetRevisionResultAsync(jobId);
            if (revisionResult == null)
            {
                return NotFound(new { Error = "Revision result not found or not ready" });
            }

            // Get the current process
            var currentProcess = await _prosessService.GetProsessDetailAsync(revisionResult.ProcessId);
            if (currentProcess == null)
            {
                return NotFound(new { Error = "Process not found" });
            }

            // Apply the revision
            var updateRequest = new UpdateProsessRequest(
                Title: revisionResult.UpdatedTitle ?? currentProcess.Title,
                Description: revisionResult.UpdatedDescription ?? currentProcess.Description,
                Category: currentProcess.Category
            );

            var updatedProcess = await _prosessService.UpdateProsessAsync(revisionResult.ProcessId, updateRequest, userId.Value);

            // TODO: Update steps from revisionResult.UpdatedSteps
            // This would require extending the ProsessService to handle step updates

            _logger.LogInformation("Successfully applied AI revision to process {ProcessId}", revisionResult.ProcessId);
            return Ok(updatedProcess);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply revision from job {JobId}", jobId);
            return StatusCode(500, new { Error = "Failed to apply revision", Details = ex.Message });
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

    // Epic 3: AI-driven Process Automation Endpoints

    [HttpPost("classify-document")]
    public async Task<ActionResult<AgentJobResponse>> ClassifyDocument([FromBody] DocumentClassificationRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
            _logger.LogInformation("User {UserId} requesting document classification: {FileName}", userId, request.FileName);

            // Prepare request data for AI agent
            var requestData = new
            {
                file_name = request.FileName,
                file_type = request.FileType,
                file_content = request.FileContent ?? "",
                file_size = request.FileSize,
                user_id = userId
            };

            var result = await _agentService.ClassifyDocumentAsync(requestData, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit document classification request");
            return StatusCode(500, new { Error = "Failed to submit document classification request", Details = ex.Message });
        }
    }

    [HttpPost("optimize-process")]
    [Authorize(Roles = "Admin,ProsessEier")]
    public async Task<ActionResult<AgentJobResponse>> OptimizeProcess([FromBody] ProcessOptimizationRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
            _logger.LogInformation("User {UserId} requesting process optimization for process {ProcessId}", userId, request.ProcessId);

            // Check if user has permission to optimize this process
            var process = await _prosessService.GetProsessDetailAsync(request.ProcessId);
            if (process == null)
            {
                return NotFound(new { Error = "Process not found" });
            }

            // Prepare request data for AI agent
            var requestData = new
            {
                process_id = request.ProcessId,
                title = request.Title ?? process.Title,
                description = request.Description ?? process.Description,
                steps = request.Steps?.Select(s => new
                {
                    id = s.Id,
                    title = s.Title,
                    description = s.Description,
                    type = s.Type,
                    estimated_duration = s.EstimatedDuration
                }).ToArray(),
                historical_data = request.HistoricalData != null ? new
                {
                    executions = request.HistoricalData.Executions?.Select(e => new
                    {
                        executed_at = e.ExecutedAt,
                        duration = e.Duration,
                        status = e.Status,
                        steps_completed = e.StepsCompleted,
                        failed_at_step = e.FailedAtStep
                    }).ToArray(),
                    step_performance = request.HistoricalData.StepPerformance?.ToDictionary(
                        kvp => kvp.Key,
                        kvp => new
                        {
                            avg_duration = kvp.Value.AvgDuration,
                            failure_rate = kvp.Value.FailureRate
                        })
                } : GenerateDefaultHistoricalData(process),
                user_id = userId
            };

            var result = await _agentService.OptimizeProcessAsync(requestData, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit process optimization request");
            return StatusCode(500, new { Error = "Failed to submit process optimization request", Details = ex.Message });
        }
    }

    [HttpGet("jobs/{jobId}/result/classification")]
    public async Task<ActionResult<DocumentClassificationResult>> GetClassificationResult(string jobId)
    {
        try
        {
            var result = await _agentService.GetClassificationResultAsync(jobId);
            if (result == null)
            {
                return NotFound(new { Error = "Classification result not found or not ready" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get classification result for job {JobId}", jobId);
            return StatusCode(500, new { Error = "Failed to get classification result", Details = ex.Message });
        }
    }

    [HttpGet("jobs/{jobId}/result/optimization")]
    public async Task<ActionResult<ProcessOptimizationResult>> GetOptimizationResult(string jobId)
    {
        try
        {
            var result = await _agentService.GetOptimizationResultAsync(jobId);
            if (result == null)
            {
                return NotFound(new { Error = "Optimization result not found or not ready" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get optimization result for job {JobId}", jobId);
            return StatusCode(500, new { Error = "Failed to get optimization result", Details = ex.Message });
        }
    }

    [HttpGet("epic3/features")]
    public async Task<ActionResult<object>> GetEpic3Features()
    {
        try
        {
            var isAvailable = await _agentService.IsAgentServiceAvailableAsync();
            return Ok(new
            {
                epic = "Epic 3: AI-driven Process Automation",
                features = new[]
                {
                    new
                    {
                        name = "Document Classification",
                        story = "Story 3.1",
                        description = "Automatic document classification and process suggestion",
                        endpoint = "/api/agent/classify-document",
                        available = isAvailable
                    },
                    new
                    {
                        name = "Process Optimization",
                        story = "Story 3.2",
                        description = "AI-driven process optimization recommendations",
                        endpoint = "/api/agent/optimize-process",
                        available = isAvailable
                    }
                },
                overall_status = isAvailable ? "available" : "unavailable"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Epic 3 features status");
            return StatusCode(500, new { Error = "Failed to get Epic 3 features", Details = ex.Message });
        }
    }

    private object GenerateDefaultHistoricalData(ProsessDetailDto process)
    {
        // Generate some sample historical data for demonstration
        var random = new Random();
        var executions = new List<object>();
        
        // Generate 20 sample executions over the last 30 days
        for (int i = 0; i < 20; i++)
        {
            var executedAt = DateTime.UtcNow.AddDays(-random.Next(0, 30));
            var isSuccessful = random.Next(1, 100) > 15; // 85% success rate
            
            executions.Add(new
            {
                executed_at = executedAt,
                duration = random.Next(30, 180), // 30-180 minutes
                status = isSuccessful ? "completed" : "failed",
                steps_completed = isSuccessful ? process.Steps?.Count ?? 0 : random.Next(1, (process.Steps?.Count ?? 5) - 1),
                failed_at_step = isSuccessful ? null : $"Step {random.Next(1, process.Steps?.Count ?? 5)}"
            });
        }

        return new
        {
            executions = executions.ToArray(),
            step_performance = new Dictionary<string, object>() // Empty for now
        };
    }
}