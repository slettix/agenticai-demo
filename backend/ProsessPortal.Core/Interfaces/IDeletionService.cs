using ProsessPortal.Core.DTOs;

namespace ProsessPortal.Core.Interfaces;

public interface IDeletionService
{
    Task<bool> CanUserDeleteProcessAsync(int userId, int prosessId);
    Task<bool> HasActiveInstancesAsync(int prosessId);
    Task<bool> SoftDeleteProcessAsync(int prosessId, int userId, DeleteProsessRequest request);
    Task<bool> HardDeleteProcessAsync(int prosessId, int userId, DeleteProsessRequest request);
    Task<bool> RestoreProcessAsync(int prosessId, int userId, RestoreProsessRequest request);
    Task<PagedResult<DeletedProsessDto>> GetDeletedProcessesAsync(int userId, int page = 1, int pageSize = 20);
    Task<BulkDeleteResult> BulkDeleteProcessesAsync(ICollection<int> prosessIds, int userId, BulkDeleteRequest request);
    Task<ICollection<DeletionHistoryDto>> GetDeletionHistoryAsync(int prosessId);
    Task LogDeletionActionAsync(int prosessId, int userId, string action, string? reason = null);
}