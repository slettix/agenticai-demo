using ProsessPortal.Core.DTOs;
using ProsessPortal.Core.Entities;

namespace ProsessPortal.Core.Interfaces;

public interface IEditingService
{
    // Edit session management
    Task<StartEditSessionResponse> StartEditSessionAsync(int prosessId, int userId, StartEditSessionRequest request);
    Task<ProsessEditSessionDto?> GetEditSessionAsync(string sessionId);
    Task<ICollection<ProsessEditSessionDto>> GetActiveEditSessionsAsync(int prosessId);
    Task EndEditSessionAsync(string sessionId, int userId, string? completionComment = null);
    Task<bool> IsUserEditingProcessAsync(int prosessId, int userId);
    
    // Draft and auto-save functionality
    Task<ProsessDetailDto> SaveDraftAsync(string sessionId, SaveDraftRequest request, int userId);
    Task<bool> AutoSaveAsync(string sessionId, SaveDraftRequest request, int userId);
    Task<ProsessDetailDto?> GetDraftAsync(string sessionId);
    Task<bool> DiscardDraftAsync(string sessionId, int userId);
    
    // Version comparison and diff
    Task<ProsessDiffDto> CompareProsessVersionsAsync(ProsessVersionCompareRequest request);
    Task<ProsessDiffDto> CompareWithDraftAsync(string sessionId, int compareWithVersionId);
    Task<ICollection<ProsessFieldChangeDto>> GetChangesSinceVersionAsync(string sessionId, int versionId);
    
    // Advanced editing with versioning
    Task<ProsessDetailDto> CompleteEditWithNewVersionAsync(string sessionId, EditProsessRequest request, int userId);
    Task<bool> CanUserEditProcessAsync(int prosessId, int userId);
    
    // Conflict detection and resolution
    Task<EditConflictDto?> DetectEditConflictAsync(int prosessId, string sessionId);
    Task<bool> ResolveConflictAsync(int conflictId, ResolveConflictRequest request, int userId);
    Task<ICollection<ProsessEditConflict>> GetActiveConflictsAsync(int prosessId);
    
    // Locking mechanism
    Task<ProsessLockDto?> AcquireLockAsync(int prosessId, string sessionId, int userId);
    Task<bool> ReleaseLockAsync(int prosessId, string sessionId, int userId);
    Task<ProsessLockDto?> GetLockStatusAsync(int prosessId);
    Task<bool> ExtendLockAsync(string sessionId, int userId);
    
    // Cleanup and maintenance
    Task CleanupExpiredSessionsAsync();
    Task<EditingStatisticsDto> GetEditingStatisticsAsync(int userId);
    Task<ICollection<ProsessAutoSave>> GetAutoSaveHistoryAsync(string sessionId);
    
    // Undo/Redo functionality
    Task<bool> UndoLastChangeAsync(string sessionId, int userId);
    Task<bool> RedoLastChangeAsync(string sessionId, int userId);
    Task<ICollection<string>> GetUndoHistoryAsync(string sessionId);
}