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

    public async Task<ProsessCategoriesDto> GetCategoriesWithITILAsync()
    {
        var businessCategories = await GetCategoriesAsync();
        var itilAreas = await GetITILAreasAsync();
        var priorities = new[] { "Lav", "Medium", "Høy", "Kritisk" };

        return new ProsessCategoriesDto(
            businessCategories,
            itilAreas,
            priorities
        );
    }

    public async Task<ICollection<ITILAreaDto>> GetITILAreasAsync()
    {
        // ITIL 4 Service Value Chain areas
        return await Task.FromResult(new List<ITILAreaDto>
        {
            new("Service Strategy", 
                "Strategisk tilnærming til service management og forretningsverdi",
                new[] { "Service Portfolio Management", "Financial Management", "Demand Management", "Business Relationship Management" }),
            
            new("Service Design", 
                "Design av IT-tjenester og støtteprosesser for å møte forretningsbehov",
                new[] { "Service Level Management", "Capacity Management", "Availability Management", "IT Service Continuity Management", "Information Security Management" }),
            
            new("Service Transition", 
                "Kontrollert overgang fra design til produksjon",
                new[] { "Change Management", "Service Asset & Configuration Management", "Release & Deployment Management", "Knowledge Management" }),
            
            new("Service Operation", 
                "Daglig drift og vedlikehold av IT-tjenester",
                new[] { "Incident Management", "Problem Management", "Event Management", "Access Management", "Request Fulfillment" }),
            
            new("Continual Service Improvement", 
                "Kontinuerlig forbedring av tjenester og prosesser",
                new[] { "Service Measurement & Reporting", "Service Improvement Planning", "ROI Analysis" })
        });
    }

    public async Task<ICollection<ITILProcessTemplateDto>> GetITILTemplatesAsync(string? area = null)
    {
        var templates = new List<ITILProcessTemplateDto>
        {
            new("Incident Management",
                "Service Operation",
                "Gjenopprette normal tjenesteproduksjon så raskt som mulig og minimere negative konsekvenser for forretningen",
                "Håndtering av alle hendelser som forstyrrer eller kan forstyrre normal tjenesteproduksjon",
                new[] { "Identifikasjon og registrering", "Kategorisering og prioritering", "Undersøkelse og diagnose", "Løsning og gjenoppretting", "Lukking" },
                new[] { "Hendelsesrapport", "Overvåkingsdata", "Brukerhenvendelser" },
                new[] { "Løst hendelse", "Problemrapport", "Endringsbehov", "Kunnskapsdatabase-oppføring" },
                new[] { "Mean Time to Resolve (MTTR)", "First Call Resolution Rate", "Customer Satisfaction Score", "Incident Volume" },
                new List<CreateProsessStepRequest>
                {
                    new("Hendelse identifisert og registrert", "Identifiser og registrer hendelsen i ITSM-verktøy", "Fyll ut alle obligatoriske felt i hendelsesregistrering", 1, StepType.Task, "Service Desk", 5, false, null),
                    new("Kategorisering og prioritering", "Kategoriser hendelsen og sett prioritet basert på påvirkning og hastighet", "Bruk kategoriseringsmatrise for å bestemme riktig prioritet", 2, StepType.Decision, "Service Desk", 10, false, null),
                    new("Første nivå støtte", "Forsøk å løse hendelsen på første nivå", "Følg kjente prosedyrer og søk i kunnskapsdatabase", 3, StepType.Task, "Service Desk", 15, false, null),
                    new("Eskalering hvis nødvendig", "Eskaler til andre nivå hvis hendelsen ikke kan løses", "Eskalering basert på prioritet og kompleksitet", 4, StepType.Decision, "Service Desk", 5, true, null),
                    new("Hendelse løst og lukket", "Bekreft løsning med bruker og lukk hendelsen", "Få bekreftelse fra bruker før lukking", 5, StepType.Task, "Service Desk", 10, false, null)
                }),

            new("Change Management",
                "Service Transition", 
                "Kontrollere livssyklusen til alle endringer for å muliggjøre fordelaktige endringer med minimum risiko",
                "Standardisert metode og prosedyre for effektiv og rask håndtering av alle endringer",
                new[] { "Endringsplanlegging", "Endringsautorisering", "Endringsimplementering", "Evaluering" },
                new[] { "Endringsbehov", "RFC (Request for Change)", "Endringsplan" },
                new[] { "Autorisert endring", "Implementert endring", "Endringsrapport" },
                new[] { "Change Success Rate", "Emergency Changes Percentage", "Changes Causing Incidents", "Average Change Lead Time" },
                new List<CreateProsessStepRequest>
                {
                    new("RFC opprettelse", "Opprett Request for Change (RFC)", "Fyll ut alle nødvendige detaljer i RFC-skjema", 1, StepType.Document, "Change Requester", 30, false, null),
                    new("Endringsvurdering", "Vurder risiko, påvirkning og ressursbehov", "Utfør impact assessment og risk analysis", 2, StepType.Task, "Change Manager", 60, false, null),
                    new("CAB gjennomgang", "Change Advisory Board gjennomgang og godkjenning", "Presenter endringen for CAB og få godkjenning", 3, StepType.Approval, "Change Advisory Board", 120, false, null),
                    new("Implementeringsplanlegging", "Planlegg implementering og tilbakerulle-plan", "Detaljert plan for implementering og tilbakerulle", 4, StepType.Task, "Change Manager", 90, false, null),
                    new("Implementering", "Gjennomfør endringen i henhold til plan", "Følg implementeringsplan nøye og dokumenter prosess", 5, StepType.Task, "Implementation Team", 180, false, null),
                    new("Post-implementation review", "Evaluér endringen og dokumenter resultater", "Vurder om endringen var vellykket og lærdom", 6, StepType.Task, "Change Manager", 45, false, null)
                }),

            new("Problem Management", 
                "Service Operation",
                "Redusere sannsynligheten for og påvirkningen av hendelser ved å identifisere faktiske og potensielle årsaker",
                "Proaktiv identifikasjon og reaktiv løsning av problemer og kjente feil",
                new[] { "Problemidentifikasjon", "Problemkontroll", "Feilkontroll" },
                new[] { "Hendelsesdata", "Overvåkingsdata", "Problemrapporter" },
                new[] { "Løst problem", "Kjent feil", "Endringsbehov", "Problemrapport" },
                new[] { "Problem Resolution Time", "Problem Prevention Rate", "Known Error Success Rate" },
                new List<CreateProsessStepRequest>
                {
                    new("Problemidentifikasjon", "Identifiser og registrer problemet", "Analyser hendelsesmønstre for å identifisere underliggende problemer", 1, StepType.Task, "Problem Manager", 30, false, null),
                    new("Problemkategorisering", "Kategoriser og prioriter problemet", "Klassifiser problem basert på påvirkning og hastighet", 2, StepType.Decision, "Problem Manager", 15, false, null),
                    new("Undersøkelse og diagnose", "Utfør detaljert undersøkelse av problemet", "Bruk problemløsningsmetoder for å finne rotårsak", 3, StepType.Task, "Technical Team", 240, false, null),
                    new("Workaround dokumentering", "Dokumenter midlertidige løsninger", "Opprett kjent feil-oppføring med workaround", 4, StepType.Document, "Problem Manager", 45, true, null),
                    new("Problemløsning", "Implementer permanent løsning", "Utarbeid RFC for permanent løsning", 5, StepType.Task, "Technical Team", 360, false, null)
                })
        };

        if (!string.IsNullOrEmpty(area))
        {
            templates = templates.Where(t => t.ITILArea.Equals(area, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return await Task.FromResult(templates);
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