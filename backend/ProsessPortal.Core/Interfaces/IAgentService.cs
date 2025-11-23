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

    // Epic 3: AI-driven Process Automation
    Task<AgentJobResponse> ClassifyDocumentAsync(object requestData, string userId);
    Task<AgentJobResponse> OptimizeProcessAsync(object requestData, string userId);
    Task<DocumentClassificationResult?> GetClassificationResultAsync(string jobId);
    Task<ProcessOptimizationResult?> GetOptimizationResultAsync(string jobId);
}