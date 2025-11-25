using Microsoft.EntityFrameworkCore;
using ProsessPortal.Core.DTOs;
using ProsessPortal.Core.Entities;
using ProsessPortal.Core.Interfaces;
using ProsessPortal.Infrastructure.Data;

namespace ProsessPortal.Infrastructure.Services;

public class DeletionService : IDeletionService
{
    private readonly ProsessPortalDbContext _context;

    public DeletionService(ProsessPortalDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CanUserDeleteProcessAsync(int userId, int prosessId)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

        if (user == null) return false;

        var prosess = await _context.Prosesser
            .FirstOrDefaultAsync(p => p.Id == prosessId);

        if (prosess == null) return false;

        // Check if user has delete_prosess permission (admin)
        var hasDeletePermission = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Any(rp => rp.Permission.Name == PermissionNames.DeleteProsess);

        if (hasDeletePermission) return true;

        // Check if user is the creator or owner
        return prosess.CreatedByUserId == userId || prosess.OwnerId == userId;
    }

    public async Task<bool> HasActiveInstancesAsync(int prosessId)
    {
        // For now, we'll assume no active instances tracking is implemented
        // This could be expanded later to check for running process instances
        await Task.CompletedTask;
        return false;
    }

    public async Task<bool> SoftDeleteProcessAsync(int prosessId, int userId, DeleteProsessRequest request)
    {
        var prosess = await _context.Prosesser
            .FirstOrDefaultAsync(p => p.Id == prosessId && !p.IsDeleted);

        if (prosess == null) return false;

        if (!await CanUserDeleteProcessAsync(userId, prosessId))
            throw new UnauthorizedAccessException("Du har ikke tilgang til å slette denne prosessen");

        if (!request.ForceDelete && await HasActiveInstancesAsync(prosessId))
            throw new InvalidOperationException("Prosessen har aktive instanser. Bruk ForceDelete for å overstyre.");

        // Perform soft delete
        prosess.IsDeleted = true;
        prosess.DeletedAt = DateTime.UtcNow;
        prosess.DeletedByUserId = userId;
        prosess.Status = ProsessStatus.Deleted;
        prosess.UpdatedAt = DateTime.UtcNow;

        // Log the action
        await LogDeletionActionAsync(prosessId, userId, DeletionActions.SoftDelete, request.Reason);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HardDeleteProcessAsync(int prosessId, int userId, DeleteProsessRequest request)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

        if (user == null) return false;

        // Only system administrators can hard delete
        var isSysAdmin = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Any(rp => rp.Permission.Name == PermissionNames.ManageUsers); // Using ManageUsers as proxy for system admin

        if (!isSysAdmin)
            throw new UnauthorizedAccessException("Kun systemadministratorer kan utføre permanent sletting");

        var prosess = await _context.Prosesser
            .Include(p => p.Tags)
            .Include(p => p.Steps)
            .Include(p => p.Versions)
            .FirstOrDefaultAsync(p => p.Id == prosessId);

        if (prosess == null) return false;

        // Log the action before deletion
        await LogDeletionActionAsync(prosessId, userId, DeletionActions.HardDelete, request.Reason);

        // Hard delete all related data
        _context.ProsessTags.RemoveRange(prosess.Tags);
        _context.ProsessSteps.RemoveRange(prosess.Steps);
        _context.ProsessVersions.RemoveRange(prosess.Versions);

        // Remove any approval requests
        var approvalRequests = await _context.ProsessApprovalRequests
            .Where(r => r.ProsessId == prosessId)
            .ToListAsync();
        _context.ProsessApprovalRequests.RemoveRange(approvalRequests);

        // Remove any edit sessions
        var editSessions = await _context.ProsessEditSessions
            .Where(s => s.ProsessId == prosessId)
            .ToListAsync();
        _context.ProsessEditSessions.RemoveRange(editSessions);

        // Finally remove the process
        _context.Prosesser.Remove(prosess);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RestoreProcessAsync(int prosessId, int userId, RestoreProsessRequest request)
    {
        var prosess = await _context.Prosesser
            .FirstOrDefaultAsync(p => p.Id == prosessId && p.IsDeleted);

        if (prosess == null) return false;

        if (!await CanUserDeleteProcessAsync(userId, prosessId))
            throw new UnauthorizedAccessException("Du har ikke tilgang til å gjenopprette denne prosessen");

        // Restore the process
        prosess.IsDeleted = false;
        prosess.DeletedAt = null;
        prosess.DeletedByUserId = null;
        prosess.Status = ProsessStatus.Draft; // Restored processes go to Draft
        prosess.UpdatedAt = DateTime.UtcNow;

        // Log the action
        await LogDeletionActionAsync(prosessId, userId, DeletionActions.Restore, request.Reason);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PagedResult<DeletedProsessDto>> GetDeletedProcessesAsync(int userId, int page = 1, int pageSize = 20)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

        if (user == null)
            throw new UnauthorizedAccessException("Bruker ikke funnet");

        var query = _context.Prosesser
            .Include(p => p.DeletedByUser)
            .Where(p => p.IsDeleted);

        // Non-admin users can only see their own deleted processes
        var isAdmin = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Any(rp => rp.Permission.Name == PermissionNames.DeleteProsess);

        if (!isAdmin)
        {
            query = query.Where(p => p.CreatedByUserId == userId || p.OwnerId == userId);
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var prosesses = await query
            .OrderByDescending(p => p.DeletedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = new List<DeletedProsessDto>();
        foreach (var p in prosesses)
        {
            var canRestore = await CanUserDeleteProcessAsync(userId, p.Id);
            items.Add(new DeletedProsessDto(
                p.Id,
                p.Title,
                p.Description,
                p.Category,
                p.Status,
                p.DeletedAt!.Value,
                $"{p.DeletedByUser!.FirstName} {p.DeletedByUser.LastName}",
                null, // Reason would come from deletion history
                canRestore
            ));
        }

        return new PagedResult<DeletedProsessDto>(items, totalCount, page, pageSize, totalPages);
    }

    public async Task<BulkDeleteResult> BulkDeleteProcessesAsync(ICollection<int> prosessIds, int userId, BulkDeleteRequest request)
    {
        var errors = new List<string>();
        int successCount = 0;

        foreach (var prosessId in prosessIds)
        {
            try
            {
                var deleteRequest = new DeleteProsessRequest(request.Reason, request.ForceDelete);
                var success = await SoftDeleteProcessAsync(prosessId, userId, deleteRequest);
                if (success) successCount++;
                else errors.Add($"Prosess {prosessId}: Kunne ikke slettes");
            }
            catch (Exception ex)
            {
                errors.Add($"Prosess {prosessId}: {ex.Message}");
            }
        }

        // Log bulk action
        await LogDeletionActionAsync(0, userId, DeletionActions.BulkDelete, 
            $"Bulk delete: {successCount}/{prosessIds.Count} successful. Reason: {request.Reason}");

        return new BulkDeleteResult(
            prosessIds.Count,
            successCount,
            prosessIds.Count - successCount,
            errors
        );
    }

    public async Task<ICollection<DeletionHistoryDto>> GetDeletionHistoryAsync(int prosessId)
    {
        var history = await _context.ProsessDeletionHistory
            .Include(h => h.Prosess)
            .Include(h => h.User)
            .Where(h => h.ProsessId == prosessId)
            .OrderByDescending(h => h.ActionAt)
            .ToListAsync();

        return history.Select(h => new DeletionHistoryDto(
            h.Id,
            h.ProsessId,
            h.Prosess.Title,
            $"{h.User.FirstName} {h.User.LastName}",
            h.ActionAt,
            h.Reason,
            null, // RestoredAt - would need to correlate with restore actions
            null, // RestoredByUser
            null  // RestoreReason
        )).ToList();
    }

    public async Task LogDeletionActionAsync(int prosessId, int userId, string action, string? reason = null)
    {
        var historyEntry = new ProsessDeletionHistory
        {
            ProsessId = prosessId,
            UserId = userId,
            Action = action,
            Reason = reason,
            ActionAt = DateTime.UtcNow
        };

        _context.ProsessDeletionHistory.Add(historyEntry);
        // Don't save here - let the calling method handle the transaction
    }
}