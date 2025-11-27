using Microsoft.EntityFrameworkCore;
using ProsessPortal.Core.DTOs;
using ProsessPortal.Core.Entities;
using ProsessPortal.Core.Interfaces;
using ProsessPortal.Infrastructure.Data;

namespace ProsessPortal.Infrastructure.Services;

public class ActorService : IActorService
{
    private readonly ProsessPortalDbContext _context;

    public ActorService(ProsessPortalDbContext context)
    {
        _context = context;
    }

    public async Task<ActorListDTO> GetActorsAsync(ActorSearchDTO search)
    {
        var query = _context.Actors.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(search.SearchTerm))
        {
            var searchLower = search.SearchTerm.ToLower();
            query = query.Where(a =>
                (a.FirstName != null && a.FirstName.ToLower().Contains(searchLower)) ||
                (a.LastName != null && a.LastName.ToLower().Contains(searchLower)) ||
                a.Email.ToLower().Contains(searchLower) ||
                (a.OrganizationName != null && a.OrganizationName.ToLower().Contains(searchLower)) ||
                (a.UnitName != null && a.UnitName.ToLower().Contains(searchLower)) ||
                (a.Department != null && a.Department.ToLower().Contains(searchLower)));
        }

        if (search.ActorCategory.HasValue)
        {
            query = query.Where(a => a.ActorCategory == search.ActorCategory.Value);
        }

        if (search.ActorType.HasValue)
        {
            query = query.Where(a => a.ActorType == search.ActorType.Value);
        }

        if (search.SecurityClearance.HasValue)
        {
            query = query.Where(a => a.SecurityClearance == search.SecurityClearance.Value);
        }

        if (!string.IsNullOrWhiteSpace(search.OrganizationName))
        {
            query = query.Where(a => a.OrganizationName == search.OrganizationName);
        }

        if (!string.IsNullOrWhiteSpace(search.Department))
        {
            query = query.Where(a => a.Department == search.Department);
        }

        if (!string.IsNullOrWhiteSpace(search.UnitName))
        {
            query = query.Where(a => a.UnitName == search.UnitName);
        }

        if (!string.IsNullOrWhiteSpace(search.GeographicLocation))
        {
            query = query.Where(a => a.GeographicLocation == search.GeographicLocation);
        }

        if (search.IsActive.HasValue)
        {
            query = query.Where(a => a.IsActive == search.IsActive.Value);
        }


        var totalCount = await query.CountAsync();

        var actors = await query
            .Include(a => a.CreatedByUser)
            .Include(a => a.UpdatedByUser)
            .Include(a => a.ActorRoles)
                .ThenInclude(ar => ar.Role)
            .OrderBy(a => a.LastName)
            .ThenBy(a => a.FirstName)
            .Skip((search.Page - 1) * search.PageSize)
            .Take(search.PageSize)
            .ToListAsync();

        var actorDTOs = actors.Select(MapToDTO).ToList();

        return new ActorListDTO
        {
            Actors = actorDTOs,
            TotalCount = totalCount,
            Page = search.Page,
            PageSize = search.PageSize
        };
    }

    public async Task<ActorDTO?> GetActorByIdAsync(int id)
    {
        var actor = await _context.Actors
            .Include(a => a.CreatedByUser)
            .Include(a => a.UpdatedByUser)
            .Include(a => a.ActorRoles)
                .ThenInclude(ar => ar.Role)
            .Include(a => a.ActorRoles)
                .ThenInclude(ar => ar.AssignedByUser)
            .Include(a => a.Notes)
                .ThenInclude(n => n.CreatedByUser)
            .FirstOrDefaultAsync(a => a.Id == id);

        return actor == null ? null : MapToDTO(actor);
    }

    public async Task<ActorDTO?> GetActorByEmailAsync(string email)
    {
        var actor = await _context.Actors
            .Include(a => a.CreatedByUser)
            .Include(a => a.UpdatedByUser)
            .Include(a => a.ActorRoles)
                .ThenInclude(ar => ar.Role)
            .FirstOrDefaultAsync(a => a.Email == email);

        return actor == null ? null : MapToDTO(actor);
    }

    public async Task<ActorDTO> CreateActorAsync(CreateActorDTO createActor, int currentUserId)
    {
        var actor = new Actor
        {
            ActorCategory = createActor.ActorCategory,
            FirstName = createActor.FirstName,
            LastName = createActor.LastName,
            OrganizationName = createActor.OrganizationName,
            UnitName = createActor.UnitName,
            UnitType = createActor.UnitType,
            UnitCode = createActor.UnitCode,
            Email = createActor.Email,
            Phone = createActor.Phone,
            ActorType = createActor.ActorType,
            SecurityClearance = createActor.SecurityClearance,
            Department = createActor.Department,
            Position = createActor.Position,
            ManagerName = createActor.ManagerName,
            ManagerEmail = createActor.ManagerEmail,
            GeographicLocation = createActor.GeographicLocation,
            Address = createActor.Address,
            PreferredLanguage = createActor.PreferredLanguage,
            ContractNumber = createActor.ContractNumber,
            ContractStartDate = createActor.ContractStartDate?.ToUniversalTime(),
            ContractEndDate = createActor.ContractEndDate?.ToUniversalTime(),
            VendorId = createActor.VendorId,
            RegistrationNumber = createActor.RegistrationNumber,
            ParentOrganization = createActor.ParentOrganization,
            EmployeeCount = createActor.EmployeeCount,
            CommandStructure = createActor.CommandStructure,
            UnitMission = createActor.UnitMission,
            PersonnelCount = createActor.PersonnelCount,
            CreatedByUserId = currentUserId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Actors.Add(actor);
        await _context.SaveChangesAsync();

        return await GetActorByIdAsync(actor.Id) ?? throw new InvalidOperationException("Failed to create actor");
    }

    public async Task<ActorDTO> UpdateActorAsync(int id, UpdateActorDTO updateActor, int currentUserId)
    {
        var actor = await _context.Actors.FindAsync(id);
        if (actor == null)
            throw new ArgumentException("Actor not found");

        actor.ActorCategory = updateActor.ActorCategory;
        actor.FirstName = updateActor.FirstName;
        actor.LastName = updateActor.LastName;
        actor.OrganizationName = updateActor.OrganizationName;
        actor.UnitName = updateActor.UnitName;
        actor.UnitType = updateActor.UnitType;
        actor.UnitCode = updateActor.UnitCode;
        actor.Email = updateActor.Email;
        actor.Phone = updateActor.Phone;
        actor.ActorType = updateActor.ActorType;
        actor.SecurityClearance = updateActor.SecurityClearance;
        actor.Department = updateActor.Department;
        actor.Position = updateActor.Position;
        actor.ManagerName = updateActor.ManagerName;
        actor.ManagerEmail = updateActor.ManagerEmail;
        actor.GeographicLocation = updateActor.GeographicLocation;
        actor.Address = updateActor.Address;
        actor.PreferredLanguage = updateActor.PreferredLanguage;
        actor.ContractNumber = updateActor.ContractNumber;
        actor.ContractStartDate = updateActor.ContractStartDate?.ToUniversalTime();
        actor.ContractEndDate = updateActor.ContractEndDate?.ToUniversalTime();
        actor.VendorId = updateActor.VendorId;
        actor.RegistrationNumber = updateActor.RegistrationNumber;
        actor.ParentOrganization = updateActor.ParentOrganization;
        actor.EmployeeCount = updateActor.EmployeeCount;
        actor.CommandStructure = updateActor.CommandStructure;
        actor.UnitMission = updateActor.UnitMission;
        actor.PersonnelCount = updateActor.PersonnelCount;
        actor.IsActive = updateActor.IsActive;
        actor.UpdatedByUserId = currentUserId;
        actor.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetActorByIdAsync(id) ?? throw new InvalidOperationException("Failed to update actor");
    }

    public async Task<bool> DeleteActorAsync(int id)
    {
        var actor = await _context.Actors.FindAsync(id);
        if (actor == null) return false;

        _context.Actors.Remove(actor);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActivateActorAsync(int id)
    {
        var actor = await _context.Actors.FindAsync(id);
        if (actor == null) return false;

        actor.IsActive = true;
        actor.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeactivateActorAsync(int id)
    {
        var actor = await _context.Actors.FindAsync(id);
        if (actor == null) return false;

        actor.IsActive = false;
        actor.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    // Role assignment methods
    public async Task<List<RoleAssignmentDTO>> GetActorRolesAsync(int actorId)
    {
        var actorRoles = await _context.ActorRoles
            .Include(ar => ar.Role)
            .Include(ar => ar.AssignedByUser)
            .Where(ar => ar.ActorId == actorId)
            .ToListAsync();

        return actorRoles.Select(ar => new RoleAssignmentDTO
        {
            RoleId = ar.RoleId,
            RoleName = ar.Role.Name,
            RoleDescription = ar.Role.Description,
            AssignedAt = ar.AssignedAt,
            AssignedByUserName = $"{ar.AssignedByUser.FirstName} {ar.AssignedByUser.LastName}",
            ValidFrom = ar.ValidFrom,
            ValidTo = ar.ValidTo,
            IsActive = ar.IsActive,
            Notes = ar.Notes
        }).ToList();
    }

    public async Task<bool> AssignRoleToActorAsync(int actorId, AssignRoleToActorDTO assignRole, int currentUserId)
    {
        // Check if assignment already exists
        var existingAssignment = await _context.ActorRoles
            .FirstOrDefaultAsync(ar => ar.ActorId == actorId && ar.RoleId == assignRole.RoleId);

        if (existingAssignment != null)
        {
            // Update existing assignment
            existingAssignment.IsActive = true;
            existingAssignment.ValidFrom = assignRole.ValidFrom;
            existingAssignment.ValidTo = assignRole.ValidTo;
            existingAssignment.Notes = assignRole.Notes;
            existingAssignment.AssignedAt = DateTime.UtcNow;
            existingAssignment.AssignedByUserId = currentUserId;
        }
        else
        {
            // Create new assignment
            var actorRole = new ActorRole
            {
                ActorId = actorId,
                RoleId = assignRole.RoleId,
                ValidFrom = assignRole.ValidFrom,
                ValidTo = assignRole.ValidTo,
                Notes = assignRole.Notes,
                AssignedByUserId = currentUserId,
                AssignedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.ActorRoles.Add(actorRole);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveRoleFromActorAsync(int actorId, int roleId, int currentUserId)
    {
        var actorRole = await _context.ActorRoles
            .FirstOrDefaultAsync(ar => ar.ActorId == actorId && ar.RoleId == roleId);

        if (actorRole == null) return false;

        _context.ActorRoles.Remove(actorRole);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateRoleAssignmentAsync(int actorId, int roleId, AssignRoleToActorDTO updateRole, int currentUserId)
    {
        var actorRole = await _context.ActorRoles
            .FirstOrDefaultAsync(ar => ar.ActorId == actorId && ar.RoleId == roleId);

        if (actorRole == null) return false;

        actorRole.ValidFrom = updateRole.ValidFrom;
        actorRole.ValidTo = updateRole.ValidTo;
        actorRole.Notes = updateRole.Notes;
        actorRole.AssignedByUserId = currentUserId;
        actorRole.AssignedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    // Notes methods
    public async Task<List<ActorNoteDTO>> GetActorNotesAsync(int actorId, bool includePrivate = false)
    {
        var query = _context.ActorNotes
            .Include(n => n.CreatedByUser)
            .Where(n => n.ActorId == actorId);

        if (!includePrivate)
        {
            query = query.Where(n => !n.IsPrivate);
        }

        var notes = await query
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return notes.Select(n => new ActorNoteDTO
        {
            Id = n.Id,
            Note = n.Note,
            Category = n.Category,
            CreatedAt = n.CreatedAt,
            CreatedByUserName = $"{n.CreatedByUser.FirstName} {n.CreatedByUser.LastName}",
            IsPrivate = n.IsPrivate
        }).ToList();
    }

    public async Task<ActorNoteDTO> AddActorNoteAsync(int actorId, CreateActorNoteDTO createNote, int currentUserId)
    {
        var note = new ActorNote
        {
            ActorId = actorId,
            Note = createNote.Note,
            Category = createNote.Category,
            IsPrivate = createNote.IsPrivate,
            CreatedByUserId = currentUserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.ActorNotes.Add(note);
        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(currentUserId);
        return new ActorNoteDTO
        {
            Id = note.Id,
            Note = note.Note,
            Category = note.Category,
            CreatedAt = note.CreatedAt,
            CreatedByUserName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown",
            IsPrivate = note.IsPrivate
        };
    }

    public async Task<bool> DeleteActorNoteAsync(int noteId, int currentUserId)
    {
        var note = await _context.ActorNotes.FindAsync(noteId);
        if (note == null) return false;

        _context.ActorNotes.Remove(note);
        await _context.SaveChangesAsync();
        return true;
    }

    // Search and filter methods
    public async Task<List<string>> GetOrganizationsAsync()
    {
        return await _context.Actors
            .Where(a => !string.IsNullOrEmpty(a.OrganizationName) && a.IsActive)
            .Select(a => a.OrganizationName!)
            .Distinct()
            .OrderBy(org => org)
            .ToListAsync();
    }

    public async Task<List<string>> GetDepartmentsAsync()
    {
        return await _context.Actors
            .Where(a => !string.IsNullOrEmpty(a.Department) && a.IsActive)
            .Select(a => a.Department!)
            .Distinct()
            .OrderBy(dept => dept)
            .ToListAsync();
    }


    // Statistics
    public async Task<Dictionary<ActorType, int>> GetActorTypeStatsAsync()
    {
        var stats = await _context.Actors
            .Where(a => a.IsActive)
            .GroupBy(a => a.ActorType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync();

        return stats.ToDictionary(s => s.Type, s => s.Count);
    }

    public async Task<Dictionary<SecurityClearance, int>> GetSecurityClearanceStatsAsync()
    {
        var stats = await _context.Actors
            .Where(a => a.IsActive)
            .GroupBy(a => a.SecurityClearance)
            .Select(g => new { Clearance = g.Key, Count = g.Count() })
            .ToListAsync();

        return stats.ToDictionary(s => s.Clearance, s => s.Count);
    }

    private ActorDTO MapToDTO(Actor actor)
    {

        var assignedRoles = actor.ActorRoles?.Where(ar => ar.IsActive).Select(ar => new RoleAssignmentDTO
        {
            RoleId = ar.RoleId,
            RoleName = ar.Role?.Name ?? "",
            RoleDescription = ar.Role?.Description,
            AssignedAt = ar.AssignedAt,
            AssignedByUserName = ar.AssignedByUser != null ? $"{ar.AssignedByUser.FirstName} {ar.AssignedByUser.LastName}" : "",
            ValidFrom = ar.ValidFrom,
            ValidTo = ar.ValidTo,
            IsActive = ar.IsActive,
            Notes = ar.Notes
        }).ToList();

        var notes = actor.Notes?.OrderByDescending(n => n.CreatedAt).Select(n => new ActorNoteDTO
        {
            Id = n.Id,
            Note = n.Note,
            Category = n.Category,
            CreatedAt = n.CreatedAt,
            CreatedByUserName = n.CreatedByUser != null ? $"{n.CreatedByUser.FirstName} {n.CreatedByUser.LastName}" : "",
            IsPrivate = n.IsPrivate
        }).ToList();

        return new ActorDTO
        {
            Id = actor.Id,
            ActorCategory = actor.ActorCategory,
            FirstName = actor.FirstName,
            LastName = actor.LastName,
            Email = actor.Email,
            Phone = actor.Phone,
            ActorType = actor.ActorType,
            SecurityClearance = actor.SecurityClearance,
            OrganizationName = actor.OrganizationName,
            UnitName = actor.UnitName,
            UnitType = actor.UnitType,
            UnitCode = actor.UnitCode,
            Department = actor.Department,
            Position = actor.Position,
            ManagerName = actor.ManagerName,
            ManagerEmail = actor.ManagerEmail,
            GeographicLocation = actor.GeographicLocation,
            Address = actor.Address,
            PreferredLanguage = actor.PreferredLanguage,
            ContractNumber = actor.ContractNumber,
            ContractStartDate = actor.ContractStartDate,
            ContractEndDate = actor.ContractEndDate,
            VendorId = actor.VendorId,
            RegistrationNumber = actor.RegistrationNumber,
            ParentOrganization = actor.ParentOrganization,
            EmployeeCount = actor.EmployeeCount,
            CommandStructure = actor.CommandStructure,
            UnitMission = actor.UnitMission,
            PersonnelCount = actor.PersonnelCount,
            IsActive = actor.IsActive,
            CreatedAt = actor.CreatedAt,
            UpdatedAt = actor.UpdatedAt,
            CreatedByUserName = actor.CreatedByUser != null ? $"{actor.CreatedByUser.FirstName} {actor.CreatedByUser.LastName}" : "",
            UpdatedByUserName = actor.UpdatedByUser != null ? $"{actor.UpdatedByUser.FirstName} {actor.UpdatedByUser.LastName}" : null,
            AssignedRoles = assignedRoles,
            Notes = notes
        };
    }
}