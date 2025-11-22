using Microsoft.EntityFrameworkCore;
using ProsessPortal.Core.DTOs;
using ProsessPortal.Core.Entities;
using ProsessPortal.Core.Interfaces;
using ProsessPortal.Infrastructure.Data;

namespace ProsessPortal.Infrastructure.Services;

public class ProsessService : IProsessService
{
    private readonly ProsessPortalDbContext _context;

    public ProsessService(ProsessPortalDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ProsessListDto>> SearchProsessesAsync(ProsessSearchRequest request)
    {
        var query = _context.Prosesser
            .Include(p => p.CreatedByUser)
            .Include(p => p.Owner)
            .Include(p => p.Tags)
            .Where(p => p.IsActive)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(p => 
                p.Title.ToLower().Contains(search) || 
                p.Description.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(p => p.Category == request.Category);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(p => p.Status == request.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Tag))
        {
            query = query.Where(p => p.Tags.Any(t => t.Name == request.Tag));
        }

        if (!string.IsNullOrWhiteSpace(request.CreatedBy))
        {
            query = query.Where(p => p.CreatedByUser.Username == request.CreatedBy);
        }

        if (!string.IsNullOrWhiteSpace(request.Owner))
        {
            query = query.Where(p => p.Owner != null && p.Owner.Username == request.Owner);
        }

        if (request.CreatedAfter.HasValue)
        {
            query = query.Where(p => p.CreatedAt >= request.CreatedAfter.Value);
        }

        if (request.CreatedBefore.HasValue)
        {
            query = query.Where(p => p.CreatedAt <= request.CreatedBefore.Value);
        }

        // Apply sorting
        query = request.SortBy.ToLower() switch
        {
            "title" => request.SortDescending ? query.OrderByDescending(p => p.Title) : query.OrderBy(p => p.Title),
            "category" => request.SortDescending ? query.OrderByDescending(p => p.Category) : query.OrderBy(p => p.Category),
            "status" => request.SortDescending ? query.OrderByDescending(p => p.Status) : query.OrderBy(p => p.Status),
            "createdat" => request.SortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            "viewcount" => request.SortDescending ? query.OrderByDescending(p => p.ViewCount) : query.OrderBy(p => p.ViewCount),
            _ => request.SortDescending ? query.OrderByDescending(p => p.UpdatedAt) : query.OrderBy(p => p.UpdatedAt)
        };

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProsessListDto(
                p.Id,
                p.Title,
                p.Description,
                p.Category,
                p.Status,
                p.UpdatedAt,
                p.CreatedByUser.FirstName + " " + p.CreatedByUser.LastName,
                p.Owner != null ? p.Owner.FirstName + " " + p.Owner.LastName : null,
                p.ViewCount,
                p.Tags.Select(t => t.Name).ToList()
            ))
            .ToListAsync();

        return new PagedResult<ProsessListDto>(items, totalCount, request.Page, request.PageSize, totalPages);
    }

    public async Task<ProsessDetailDto?> GetProsessDetailAsync(int id)
    {
        var prosess = await _context.Prosesser
            .Include(p => p.CreatedByUser)
            .Include(p => p.Owner)
            .Include(p => p.Tags)
            .Include(p => p.Steps.OrderBy(s => s.OrderIndex))
                .ThenInclude(s => s.SubSteps.OrderBy(ss => ss.OrderIndex))
            .Include(p => p.Steps)
                .ThenInclude(s => s.OutgoingConnections)
                    .ThenInclude(c => c.ToStep)
            .Include(p => p.Versions.OrderByDescending(v => v.CreatedAt))
                .ThenInclude(v => v.CreatedByUser)
            .Include(p => p.Versions)
                .ThenInclude(v => v.PublishedByUser)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (prosess == null) return null;

        var currentVersion = prosess.Versions.FirstOrDefault(v => v.IsCurrent);
        var versionHistory = prosess.Versions.Select(v => new ProsessVersionSummaryDto(
            v.Id,
            v.VersionNumber,
            v.Title,
            v.CreatedAt,
            v.CreatedByUser.FirstName + " " + v.CreatedByUser.LastName,
            v.IsCurrent,
            v.IsPublished
        )).ToList();

        return new ProsessDetailDto(
            prosess.Id,
            prosess.Title,
            prosess.Description,
            prosess.Category,
            prosess.Status,
            prosess.GitRepository,
            prosess.GitPath,
            prosess.GitBranch,
            prosess.CreatedAt,
            prosess.UpdatedAt,
            prosess.CreatedByUser.FirstName + " " + prosess.CreatedByUser.LastName,
            prosess.Owner?.FirstName + " " + prosess.Owner?.LastName,
            prosess.ViewCount,
            prosess.LastAccessedAt,
            prosess.Tags.Select(t => new ProsessTagDto(t.Id, t.Name, t.Color)).ToList(),
            MapStepsToDto(prosess.Steps.Where(s => s.ParentStepId == null).OrderBy(s => s.OrderIndex).ToList()),
            currentVersion != null ? MapVersionToDto(currentVersion) : null,
            versionHistory
        );
    }

    public async Task<ProsessDetailDto?> CreateProsessAsync(CreateProsessRequest request, int userId)
    {
        var prosess = new Prosess
        {
            Title = request.Title,
            Description = request.Description,
            Category = request.Category,
            CreatedByUserId = userId,
            Status = ProsessStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Prosesser.Add(prosess);
        await _context.SaveChangesAsync();

        // Add tags if provided
        if (request.Tags?.Any() == true)
        {
            foreach (var tagName in request.Tags)
            {
                var tag = new ProsessTag
                {
                    ProsessId = prosess.Id,
                    Name = tagName,
                    Color = GetDefaultColorForTag(tagName)
                };
                _context.ProsessTags.Add(tag);
            }
            await _context.SaveChangesAsync();
        }

        // Create initial version
        var initialVersion = new ProsessVersion
        {
            ProsessId = prosess.Id,
            VersionNumber = "1.0.0",
            Title = request.Title,
            Description = request.Description,
            Content = "# " + request.Title + "\n\n" + request.Description,
            ChangeLog = "Initial version",
            CreatedByUserId = userId,
            IsCurrent = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.ProsessVersions.Add(initialVersion);
        await _context.SaveChangesAsync();

        return await GetProsessDetailAsync(prosess.Id);
    }

    public async Task<ProsessDetailDto?> UpdateProsessAsync(int id, UpdateProsessRequest request, int userId)
    {
        var prosess = await _context.Prosesser
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (prosess == null) return null;

        prosess.Title = request.Title;
        prosess.Description = request.Description;
        prosess.Category = request.Category;
        prosess.UpdatedAt = DateTime.UtcNow;

        // Update tags
        if (request.Tags != null)
        {
            // Remove existing tags
            _context.ProsessTags.RemoveRange(prosess.Tags);
            
            // Add new tags
            foreach (var tagName in request.Tags)
            {
                var tag = new ProsessTag
                {
                    ProsessId = prosess.Id,
                    Name = tagName,
                    Color = GetDefaultColorForTag(tagName)
                };
                _context.ProsessTags.Add(tag);
            }
        }

        await _context.SaveChangesAsync();
        return await GetProsessDetailAsync(id);
    }

    public async Task<bool> DeleteProsessAsync(int id, int userId)
    {
        var prosess = await _context.Prosesser.FindAsync(id);
        if (prosess == null) return false;

        // Soft delete
        prosess.IsActive = false;
        prosess.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ProsessStatisticsDto> GetProsessStatisticsAsync()
    {
        var prosesses = await _context.Prosesser
            .Include(p => p.CreatedByUser)
            .Include(p => p.Owner)
            .Include(p => p.Tags)
            .Where(p => p.IsActive)
            .ToListAsync();

        var totalProsesses = prosesses.Count;
        var publishedProsesses = prosesses.Count(p => p.Status == ProsessStatus.Published);
        var draftProsesses = prosesses.Count(p => p.Status == ProsessStatus.Draft);
        var inReviewProsesses = prosesses.Count(p => p.Status == ProsessStatus.InReview);

        var processesByCategory = prosesses
            .GroupBy(p => p.Category)
            .ToDictionary(g => g.Key, g => g.Count());

        var processesByStatus = prosesses
            .GroupBy(p => p.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var totalViews = prosesses.Sum(p => p.ViewCount);
        
        var mostViewed = prosesses.OrderByDescending(p => p.ViewCount).FirstOrDefault();
        var recentlyUpdated = prosesses.OrderByDescending(p => p.UpdatedAt).FirstOrDefault();

        return new ProsessStatisticsDto(
            totalProsesses,
            publishedProsesses,
            draftProsesses,
            inReviewProsesses,
            processesByCategory,
            processesByStatus,
            totalViews,
            mostViewed != null ? MapToListDto(mostViewed) : null,
            recentlyUpdated != null ? MapToListDto(recentlyUpdated) : null
        );
    }

    public async Task<ICollection<string>> GetCategoriesAsync()
    {
        return await _context.Prosesser
            .Where(p => p.IsActive)
            .Select(p => p.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    public async Task<ICollection<ProsessTagDto>> GetTagsAsync()
    {
        return await _context.ProsessTags
            .Select(t => new ProsessTagDto(t.Id, t.Name, t.Color))
            .Distinct()
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<bool> RecordProsessViewAsync(int prosessId)
    {
        var prosess = await _context.Prosesser.FindAsync(prosessId);
        if (prosess == null) return false;

        prosess.ViewCount++;
        prosess.LastAccessedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    private static ICollection<ProsessStepDto> MapStepsToDto(List<ProsessStep> steps)
    {
        return steps.Select(s => new ProsessStepDto(
            s.Id,
            s.Title,
            s.Description,
            s.DetailedInstructions,
            s.OrderIndex,
            s.Type,
            s.ResponsibleRole,
            s.EstimatedDurationMinutes,
            s.IsOptional,
            s.ParentStepId,
            MapStepsToDto(s.SubSteps.OrderBy(ss => ss.OrderIndex).ToList()),
            s.OutgoingConnections.Select(c => new StepConnectionDto(
                c.Id,
                c.ToStepId,
                c.Condition,
                c.Type
            )).ToList()
        )).ToList();
    }

    private static ProsessVersionDto MapVersionToDto(ProsessVersion version)
    {
        return new ProsessVersionDto(
            version.Id,
            version.VersionNumber,
            version.Title,
            version.Description,
            version.Content,
            version.ChangeLog,
            version.CreatedAt,
            version.CreatedByUser.FirstName + " " + version.CreatedByUser.LastName,
            version.IsCurrent,
            version.IsPublished,
            version.PublishedAt,
            version.PublishedByUser?.FirstName + " " + version.PublishedByUser?.LastName
        );
    }

    private static ProsessListDto MapToListDto(Prosess prosess)
    {
        return new ProsessListDto(
            prosess.Id,
            prosess.Title,
            prosess.Description,
            prosess.Category,
            prosess.Status,
            prosess.UpdatedAt,
            prosess.CreatedByUser.FirstName + " " + prosess.CreatedByUser.LastName,
            prosess.Owner?.FirstName + " " + prosess.Owner?.LastName,
            prosess.ViewCount,
            prosess.Tags.Select(t => t.Name).ToList()
        );
    }

    private static string GetDefaultColorForTag(string tagName)
    {
        return tagName.ToLower() switch
        {
            "kritisk" => "#dc3545",
            "automatisert" => "#28a745",
            "manuell" => "#ffc107",
            "quick win" => "#17a2b8",
            "kompleks" => "#6c757d",
            "hyppig" => "#007bff",
            "sjelden" => "#6f42c1",
            "kundevendt" => "#fd7e14",
            "intern" => "#20c997",
            "regulatorisk" => "#e83e8c",
            _ => "#007bff"
        };
    }
}