using ProsessPortal.Core.DTOs;

namespace ProsessPortal.Core.Interfaces;

public interface IProsessService
{
    Task<PagedResult<ProsessListDto>> SearchProsessesAsync(ProsessSearchRequest request);
    Task<ProsessDetailDto?> GetProsessDetailAsync(int id);
    Task<ProsessDetailDto?> CreateProsessAsync(CreateProsessRequest request, int userId);
    Task<ProsessDetailDto?> UpdateProsessAsync(int id, UpdateProsessRequest request, int userId);
    Task<bool> DeleteProsessAsync(int id, int userId);
    Task<ProsessStatisticsDto> GetProsessStatisticsAsync();
    Task<ICollection<string>> GetCategoriesAsync();
    Task<ProsessCategoriesDto> GetCategoriesWithITILAsync();
    Task<ICollection<ITILAreaDto>> GetITILAreasAsync();
    Task<ICollection<ITILProcessTemplateDto>> GetITILTemplatesAsync(string? area = null);
    Task<ICollection<ProsessTagDto>> GetTagsAsync();
    Task<bool> RecordProsessViewAsync(int prosessId);
}

public interface IProsessVersionService
{
    Task<ICollection<ProsessVersionSummaryDto>> GetVersionHistoryAsync(int prosessId);
    Task<ProsessVersionDto?> GetVersionAsync(int versionId);
    Task<ProsessVersionDto?> CreateVersionAsync(int prosessId, string versionNumber, string changeLog, int userId);
    Task<bool> PublishVersionAsync(int versionId, int userId);
    Task<bool> SetCurrentVersionAsync(int versionId, int userId);
}

public interface IProsessStepService
{
    Task<ICollection<ProsessStepDto>> GetStepsAsync(int prosessId);
    Task<ProsessStepDto?> CreateStepAsync(int prosessId, CreateProsessStepRequest request, int userId);
    Task<ProsessStepDto?> UpdateStepAsync(int stepId, CreateProsessStepRequest request, int userId);
    Task<bool> DeleteStepAsync(int stepId, int userId);
    Task<bool> ReorderStepsAsync(int prosessId, ICollection<int> stepIds, int userId);
}