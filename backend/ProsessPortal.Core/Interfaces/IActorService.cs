using ProsessPortal.Core.DTOs;
using ProsessPortal.Core.Entities;

namespace ProsessPortal.Core.Interfaces;

public interface IActorService
{
    Task<ActorListDTO> GetActorsAsync(ActorSearchDTO search);
    Task<ActorDTO?> GetActorByIdAsync(int id);
    Task<ActorDTO?> GetActorByEmailAsync(string email);
    Task<ActorDTO> CreateActorAsync(CreateActorDTO createActor, int currentUserId);
    Task<ActorDTO> UpdateActorAsync(int id, UpdateActorDTO updateActor, int currentUserId);
    Task<bool> DeleteActorAsync(int id);
    Task<bool> ActivateActorAsync(int id);
    Task<bool> DeactivateActorAsync(int id);
    
    // Role assignment methods
    Task<List<RoleAssignmentDTO>> GetActorRolesAsync(int actorId);
    Task<bool> AssignRoleToActorAsync(int actorId, AssignRoleToActorDTO assignRole, int currentUserId);
    Task<bool> RemoveRoleFromActorAsync(int actorId, int roleId, int currentUserId);
    Task<bool> UpdateRoleAssignmentAsync(int actorId, int roleId, AssignRoleToActorDTO updateRole, int currentUserId);
    
    // Notes methods
    Task<List<ActorNoteDTO>> GetActorNotesAsync(int actorId, bool includePrivate = false);
    Task<ActorNoteDTO> AddActorNoteAsync(int actorId, CreateActorNoteDTO createNote, int currentUserId);
    Task<bool> DeleteActorNoteAsync(int noteId, int currentUserId);
    
    // Search and filter methods
    Task<List<string>> GetOrganizationsAsync();
    Task<List<string>> GetDepartmentsAsync();
    Task<List<string>> GetCompetenceAreasAsync();
    Task<List<string>> GetTechnicalSkillsAsync();
    
    // Statistics
    Task<Dictionary<ActorType, int>> GetActorTypeStatsAsync();
    Task<Dictionary<SecurityClearance, int>> GetSecurityClearanceStatsAsync();
}