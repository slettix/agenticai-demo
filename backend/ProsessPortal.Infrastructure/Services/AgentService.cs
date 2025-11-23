using System.Text;
using System.Text.Json;
using ProsessPortal.Core.DTOs;
using ProsessPortal.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ProsessPortal.Infrastructure.Services;

public class AgentService : IAgentService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AgentService> _logger;
    private readonly string _agentServiceBaseUrl;

    public AgentService(HttpClient httpClient, IConfiguration configuration, ILogger<AgentService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _agentServiceBaseUrl = configuration["AgentService:BaseUrl"] ?? "http://localhost:8001";
    }

    public async Task<AgentJobResponse> GenerateProcessAsync(ProcessGenerationRequest request, string userId)
    {
        try
        {
            _logger.LogInformation("Submitting process generation request for user {UserId}", userId);

            var agentRequest = new
            {
                title = request.Title,
                description = request.Description,
                category = request.Category,
                requirements = request.Requirements,
                target_audience = request.TargetAudience,
                complexity_level = request.ComplexityLevel,
                user_id = userId
            };

            var json = JsonSerializer.Serialize(agentRequest, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_agentServiceBaseUrl}/api/agents/generate-process", content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var jobResponse = JsonSerializer.Deserialize<AgentJobResponse>(responseJson, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                _logger.LogInformation("Process generation job submitted with ID: {JobId}", jobResponse?.JobId);
                return jobResponse ?? throw new Exception("Invalid response from agent service");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Agent service returned error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                throw new Exception($"Agent service error: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit process generation request");
            throw;
        }
    }

    public async Task<AgentJobResponse> ReviseProcessAsync(ProcessRevisionRequest request, string userId)
    {
        try
        {
            _logger.LogInformation("Submitting process revision request for process {ProcessId} by user {UserId}", 
                request.ProcessId, userId);

            var agentRequest = new
            {
                process_id = request.ProcessId,
                revision_type = request.RevisionType,
                feedback = request.Feedback,
                improvement_goals = request.ImprovementGoals,
                custom_instructions = request.CustomInstructions,
                user_id = userId
            };

            var json = JsonSerializer.Serialize(agentRequest, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_agentServiceBaseUrl}/api/agents/revise-process", content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var jobResponse = JsonSerializer.Deserialize<AgentJobResponse>(responseJson, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                _logger.LogInformation("Process revision job submitted with ID: {JobId}", jobResponse?.JobId);
                return jobResponse ?? throw new Exception("Invalid response from agent service");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Agent service returned error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                throw new Exception($"Agent service error: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit process revision request");
            throw;
        }
    }

    public async Task<AgentJobStatusResponse?> GetJobStatusAsync(string jobId)
    {
        try
        {
            _logger.LogDebug("Getting job status for job {JobId}", jobId);

            var response = await _httpClient.GetAsync($"{_agentServiceBaseUrl}/api/jobs/{jobId}/status");

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var jobStatus = JsonSerializer.Deserialize<AgentJobStatusResponse>(responseJson, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                return jobStatus;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Job {JobId} not found", jobId);
                return null;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to get job status: {StatusCode} - {Error}", response.StatusCode, errorContent);
                throw new Exception($"Failed to get job status: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job status for job {JobId}", jobId);
            throw;
        }
    }

    public async Task<ProcessGenerationResult?> GetGenerationResultAsync(string jobId)
    {
        try
        {
            _logger.LogDebug("Getting generation result for job {JobId}", jobId);

            var response = await _httpClient.GetAsync($"{_agentServiceBaseUrl}/api/jobs/{jobId}/result");

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ProcessGenerationResult>(responseJson, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                return result;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Generation result for job {JobId} not found", jobId);
                return null;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to get generation result: {StatusCode} - {Error}", response.StatusCode, errorContent);
                throw new Exception($"Failed to get generation result: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting generation result for job {JobId}", jobId);
            throw;
        }
    }

    public async Task<ProcessRevisionResult?> GetRevisionResultAsync(string jobId)
    {
        try
        {
            _logger.LogDebug("Getting revision result for job {JobId}", jobId);

            var response = await _httpClient.GetAsync($"{_agentServiceBaseUrl}/api/jobs/{jobId}/result");

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ProcessRevisionResult>(responseJson, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                return result;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Revision result for job {JobId} not found", jobId);
                return null;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to get revision result: {StatusCode} - {Error}", response.StatusCode, errorContent);
                throw new Exception($"Failed to get revision result: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revision result for job {JobId}", jobId);
            throw;
        }
    }

    public async Task<bool> IsAgentServiceAvailableAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_agentServiceBaseUrl}/");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Agent service is not available");
            return false;
        }
    }
}