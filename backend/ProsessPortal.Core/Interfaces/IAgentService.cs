using ProsessPortal.Core.DTOs;

namespace ProsessPortal.Core.Interfaces;

public interface IAgentService
{
    Task<AgentJobResponse> GenerateProcessAsync(ProcessGenerationRequest request, string userId);
    Task<AgentJobResponse> ReviseProcessAsync(ProcessRevisionRequest request, string userId);
    Task<AgentJobStatusResponse?> GetJobStatusAsync(string jobId);
    Task<ProcessGenerationResult?> GetGenerationResultAsync(string jobId);
    Task<ProcessRevisionResult?> GetRevisionResultAsync(string jobId);
    Task<bool> IsAgentServiceAvailableAsync();
}