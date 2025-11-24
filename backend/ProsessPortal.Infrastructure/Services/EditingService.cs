using Microsoft.EntityFrameworkCore;
using ProsessPortal.Core.DTOs;
using ProsessPortal.Core.Entities;
using ProsessPortal.Core.Interfaces;
using ProsessPortal.Infrastructure.Data;
using System.Text.Json;

namespace ProsessPortal.Infrastructure.Services;

public class EditingService : IEditingService
{
    private readonly ProsessPortalDbContext _context;
    private readonly IProsessService _prosessService;

    public EditingService(ProsessPortalDbContext context, IProsessService prosessService)
    {
        _context = context;
        _prosessService = prosessService;
    }

    public async Task<StartEditSessionResponse> StartEditSessionAsync(int prosessId, int userId, StartEditSessionRequest request)
    {
        var prosess = await _context.Prosesser
            .Include(p => p.CreatedByUser)
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == prosessId && p.IsActive);

        if (prosess == null)
            throw new ArgumentException("Prosess ikke funnet");

        if (!await CanUserEditProcessAsync(prosessId, userId))
            throw new UnauthorizedAccessException("Du har ikke tilgang til å redigere denne prosessen");

        // Check for existing active session for this user
        var existingSession = await _context.ProsessEditSessions
            .FirstOrDefaultAsync(s => s.ProsessId == prosessId && 
                                    s.UserId == userId && 
                                    s.Status == ProsessEditStatus.Active);

        if (existingSession != null)
        {
            // Extend existing session
            existingSession.LastActivity = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        else
        {
            // Create new session
            existingSession = new ProsessEditSession
            {
                ProsessId = prosessId,
                UserId = userId,
                StartComment = request.Comment,
                Status = ProsessEditStatus.Active
            };

            _context.ProsessEditSessions.Add(existingSession);
            await _context.SaveChangesAsync();
        }

        // Get all active sessions for conflict detection
        var activeSessions = await GetActiveEditSessionsAsync(prosessId);
        var prosessDetail = await _prosessService.GetProsessDetailAsync(prosessId);

        return new StartEditSessionResponse(
            existingSession.SessionId,
            prosessDetail!,
            MapToDto(existingSession),
            activeSessions.Where(s => s.SessionId != existingSession.SessionId).ToList()
        );
    }

    public async Task<bool> CanUserEditProcessAsync(int prosessId, int userId)
    {
        var prosess = await _context.Prosesser
            .FirstOrDefaultAsync(p => p.Id == prosessId);

        if (prosess == null) return false;

        // User can edit if they are the creator or owner
        if (prosess.CreatedByUserId == userId || prosess.OwnerId == userId)
            return true;

        // Check if user has edit permissions
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return false;

        var hasEditPermission = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Any(rp => rp.Permission.Name == PermissionNames.EditProsess);

        return hasEditPermission;
    }

    public async Task<ProsessDetailDto> SaveDraftAsync(string sessionId, SaveDraftRequest request, int userId)
    {
        var session = await _context.ProsessEditSessions
            .Include(s => s.Prosess)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.UserId == userId);

        if (session == null)
            throw new ArgumentException("Ugyldig redigeringssesjon");

        // Update draft data
        session.DraftTitle = request.Title;
        session.DraftDescription = request.Description;
        session.DraftCategory = request.Category;
        session.DraftTags = request.Tags != null ? JsonSerializer.Serialize(request.Tags) : null;
        session.DraftSteps = request.Steps != null ? JsonSerializer.Serialize(request.Steps) : null;
        session.LastActivity = DateTime.UtcNow;
        session.LastAutoSave = DateTime.UtcNow;

        // Save auto-save record
        var autoSave = new ProsessAutoSave
        {
            SessionId = sessionId,
            ProsessId = session.ProsessId,
            UserId = userId,
            Content = JsonSerializer.Serialize(request)
        };

        _context.ProsessAutoSaves.Add(autoSave);
        await _context.SaveChangesAsync();

        // Return current state (mix of saved and draft data)
        return await GetDraftAsync(sessionId) ?? await _prosessService.GetProsessDetailAsync(session.ProsessId) ?? throw new InvalidOperationException("Could not retrieve process details");
    }

    public async Task<ProsessDetailDto?> GetDraftAsync(string sessionId)
    {
        var session = await _context.ProsessEditSessions
            .Include(s => s.Prosess)
                .ThenInclude(p => p.CreatedByUser)
            .Include(s => s.Prosess)
                .ThenInclude(p => p.Owner)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);

        if (session?.LastAutoSave == null)
            return null;

        // Create a draft version of the prosess detail
        var originalProsess = session.Prosess;
        var draftTags = !string.IsNullOrEmpty(session.DraftTags) 
            ? JsonSerializer.Deserialize<string[]>(session.DraftTags)?.Select(t => new ProsessTagDto(0, t, "#007bff")).ToArray() ?? Array.Empty<ProsessTagDto>()
            : Array.Empty<ProsessTagDto>();
        
        var draftSteps = !string.IsNullOrEmpty(session.DraftSteps) 
            ? JsonSerializer.Deserialize<CreateProsessStepRequest[]>(session.DraftSteps)?.Select((s, i) => new ProsessStepDto(
                i,
                s.Title,
                s.Description,
                s.DetailedInstructions,
                s.OrderIndex,
                s.Type,
                s.ResponsibleRole,
                s.EstimatedDurationMinutes,
                s.IsOptional,
                null,
                Array.Empty<ProsessStepDto>(),
                Array.Empty<StepConnectionDto>()
            )).ToArray() ?? Array.Empty<ProsessStepDto>()
            : Array.Empty<ProsessStepDto>();

        return new ProsessDetailDto(
            originalProsess.Id,
            session.DraftTitle ?? originalProsess.Title,
            session.DraftDescription ?? originalProsess.Description,
            session.DraftCategory ?? originalProsess.Category,
            originalProsess.Status,
            originalProsess.GitRepository,
            originalProsess.GitPath,
            originalProsess.GitBranch,
            originalProsess.CreatedAt,
            originalProsess.UpdatedAt,
            originalProsess.CreatedByUser.FirstName + " " + originalProsess.CreatedByUser.LastName,
            originalProsess.Owner?.FirstName + " " + originalProsess.Owner?.LastName,
            originalProsess.ViewCount,
            originalProsess.LastAccessedAt,
            draftTags,
            draftSteps,
            null,
            Array.Empty<ProsessVersionSummaryDto>()
        );
    }

    public async Task<ProsessDetailDto> CompleteEditWithNewVersionAsync(string sessionId, EditProsessRequest request, int userId)
    {
        var session = await _context.ProsessEditSessions
            .Include(s => s.Prosess)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.UserId == userId);

        if (session == null)
            throw new ArgumentException("Ugyldig redigeringssesjon");

        var prosess = session.Prosess;

        if (request.SaveAsDraft)
        {
            // Just save as draft without creating a version
            var draftRequest = new SaveDraftRequest(
                request.Title,
                request.Description,
                request.Category,
                request.ITILArea,
                request.Priority,
                request.OwnerId,
                request.Tags,
                request.Steps,
                request.ChangeComment
            );

            return await SaveDraftAsync(sessionId, draftRequest, userId);
        }

        // Create new version if this is a published process
        ProsessVersion? newVersion = null;
        if (prosess.Status == ProsessStatus.Published)
        {
            var currentVersion = await _context.ProsessVersions
                .Where(v => v.ProsessId == prosess.Id && v.IsCurrent)
                .FirstOrDefaultAsync();

            var versionNumber = currentVersion != null 
                ? VersionHelper.GetNextVersion(currentVersion.VersionNumber, request.VersionChangeType)
                : "1.0.0";

            newVersion = new ProsessVersion
            {
                ProsessId = prosess.Id,
                VersionNumber = versionNumber,
                Title = request.Title,
                Description = request.Description,
                Content = JsonSerializer.Serialize(request.Steps ?? Array.Empty<CreateProsessStepRequest>()),
                ChangeLog = request.ChangeComment ?? "Endringer fra redigeringssesjon",
                CreatedByUserId = userId,
                IsCurrent = false,
                IsPublished = false
            };

            _context.ProsessVersions.Add(newVersion);

            // Mark current version as not current
            if (currentVersion != null)
            {
                currentVersion.IsCurrent = false;
            }

            // Update process status back to draft for approval workflow
            prosess.Status = ProsessStatus.Draft;
        }

        // Update the main prosess
        prosess.Title = request.Title;
        prosess.Description = request.Description;
        prosess.Category = request.Category;
        prosess.OwnerId = request.OwnerId;
        prosess.UpdatedAt = DateTime.UtcNow;

        // Update tags
        if (request.Tags != null)
        {
            var existingTags = await _context.ProsessTags
                .Where(t => t.ProsessId == prosess.Id)
                .ToListAsync();
            
            _context.ProsessTags.RemoveRange(existingTags);
            
            foreach (var tagName in request.Tags)
            {
                _context.ProsessTags.Add(new ProsessTag
                {
                    ProsessId = prosess.Id,
                    Name = tagName,
                    Color = GetDefaultColorForTag(tagName)
                });
            }
        }

        // Update steps
        if (request.Steps != null)
        {
            var existingSteps = await _context.ProsessSteps
                .Where(s => s.ProsessId == prosess.Id)
                .ToListAsync();
            
            _context.ProsessSteps.RemoveRange(existingSteps);
            
            foreach (var stepRequest in request.Steps)
            {
                _context.ProsessSteps.Add(new ProsessStep
                {
                    ProsessId = prosess.Id,
                    Title = stepRequest.Title,
                    Description = stepRequest.Description,
                    DetailedInstructions = stepRequest.DetailedInstructions,
                    Type = stepRequest.Type,
                    ResponsibleRole = stepRequest.ResponsibleRole,
                    EstimatedDurationMinutes = stepRequest.EstimatedDurationMinutes,
                    OrderIndex = stepRequest.OrderIndex,
                    IsOptional = stepRequest.IsOptional
                });
            }
        }

        // Complete the edit session
        session.Status = ProsessEditStatus.Completed;
        session.CompletedAt = DateTime.UtcNow;
        session.CompletionComment = request.ChangeComment;
        session.CreatedVersionId = newVersion?.Id;

        await _context.SaveChangesAsync();

        return await _prosessService.GetProsessDetailAsync(prosess.Id) ?? throw new InvalidOperationException("Could not retrieve updated process details");
    }

    public async Task<ICollection<ProsessEditSessionDto>> GetActiveEditSessionsAsync(int prosessId)
    {
        var sessions = await _context.ProsessEditSessions
            .Include(s => s.User)
            .Where(s => s.ProsessId == prosessId && s.Status == ProsessEditStatus.Active)
            .OrderBy(s => s.StartedAt)
            .ToListAsync();

        return sessions.Select(MapToDto).ToList();
    }

    public async Task EndEditSessionAsync(string sessionId, int userId, string? completionComment = null)
    {
        var session = await _context.ProsessEditSessions
            .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.UserId == userId);

        if (session == null) return;

        session.Status = ProsessEditStatus.Completed;
        session.CompletedAt = DateTime.UtcNow;
        session.CompletionComment = completionComment;

        await _context.SaveChangesAsync();
    }

    // Helper methods
    private static ProsessEditSessionDto MapToDto(ProsessEditSession session)
    {
        return new ProsessEditSessionDto(
            session.ProsessId,
            session.SessionId,
            session.UserId,
            $"{session.User?.FirstName} {session.User?.LastName}".Trim(),
            session.StartedAt,
            session.LastActivity,
            session.Status == ProsessEditStatus.Active,
            session.Status
        );
    }

    private static string GetDefaultColorForTag(string tagName)
    {
        // Simple hash-based color assignment
        var hash = tagName.GetHashCode();
        var colors = new[] { "#007bff", "#28a745", "#dc3545", "#ffc107", "#17a2b8", "#6f42c1" };
        return colors[Math.Abs(hash) % colors.Length];
    }

    // Placeholder implementations for remaining methods
    public Task<bool> AutoSaveAsync(string sessionId, SaveDraftRequest request, int userId)
    {
        return SaveDraftAsync(sessionId, request, userId).ContinueWith(t => true);
    }

    public Task<bool> DiscardDraftAsync(string sessionId, int userId)
    {
        throw new NotImplementedException();
    }

    public Task<ProsessDiffDto> CompareProsessVersionsAsync(ProsessVersionCompareRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ProsessDiffDto> CompareWithDraftAsync(string sessionId, int compareWithVersionId)
    {
        throw new NotImplementedException();
    }

    public Task<ICollection<ProsessFieldChangeDto>> GetChangesSinceVersionAsync(string sessionId, int versionId)
    {
        throw new NotImplementedException();
    }

    public Task<EditConflictDto?> DetectEditConflictAsync(int prosessId, string sessionId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ResolveConflictAsync(int conflictId, ResolveConflictRequest request, int userId)
    {
        throw new NotImplementedException();
    }

    public Task<ICollection<ProsessEditConflict>> GetActiveConflictsAsync(int prosessId)
    {
        throw new NotImplementedException();
    }

    public Task<ProsessLockDto?> AcquireLockAsync(int prosessId, string sessionId, int userId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ReleaseLockAsync(int prosessId, string sessionId, int userId)
    {
        throw new NotImplementedException();
    }

    public Task<ProsessLockDto?> GetLockStatusAsync(int prosessId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExtendLockAsync(string sessionId, int userId)
    {
        throw new NotImplementedException();
    }

    public Task CleanupExpiredSessionsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<EditingStatisticsDto> GetEditingStatisticsAsync(int userId)
    {
        throw new NotImplementedException();
    }

    public Task<ICollection<ProsessAutoSave>> GetAutoSaveHistoryAsync(string sessionId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UndoLastChangeAsync(string sessionId, int userId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RedoLastChangeAsync(string sessionId, int userId)
    {
        throw new NotImplementedException();
    }

    public Task<ICollection<string>> GetUndoHistoryAsync(string sessionId)
    {
        throw new NotImplementedException();
    }

    public Task<ProsessEditSessionDto?> GetEditSessionAsync(string sessionId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsUserEditingProcessAsync(int prosessId, int userId)
    {
        throw new NotImplementedException();
    }
}