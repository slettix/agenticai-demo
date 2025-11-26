using ProsessPortal.Core.Entities;

namespace ProsessPortal.Core.DTOs;

public class ActorDTO
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public ActorType ActorType { get; set; }
    public SecurityClearance SecurityClearance { get; set; }
    public string? OrganizationName { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public string? ManagerName { get; set; }
    public string? ManagerEmail { get; set; }
    public string? GeographicLocation { get; set; }
    public string? Address { get; set; }
    public string? PreferredLanguage { get; set; }
    public List<string>? CompetenceAreas { get; set; }
    public List<string>? TechnicalSkills { get; set; }
    public string? ContractNumber { get; set; }
    public DateTime? ContractStartDate { get; set; }
    public DateTime? ContractEndDate { get; set; }
    public string? VendorId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public string? UpdatedByUserName { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public List<RoleAssignmentDTO>? AssignedRoles { get; set; }
    public List<ActorNoteDTO>? Notes { get; set; }
}

public class CreateActorDTO
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public ActorType ActorType { get; set; }
    public SecurityClearance SecurityClearance { get; set; } = SecurityClearance.None;
    public string? OrganizationName { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public string? ManagerName { get; set; }
    public string? ManagerEmail { get; set; }
    public string? GeographicLocation { get; set; }
    public string? Address { get; set; }
    public string? PreferredLanguage { get; set; } = "NO";
    public List<string>? CompetenceAreas { get; set; }
    public List<string>? TechnicalSkills { get; set; }
    public string? ContractNumber { get; set; }
    public DateTime? ContractStartDate { get; set; }
    public DateTime? ContractEndDate { get; set; }
    public string? VendorId { get; set; }
}

public class UpdateActorDTO
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public ActorType ActorType { get; set; }
    public SecurityClearance SecurityClearance { get; set; }
    public string? OrganizationName { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public string? ManagerName { get; set; }
    public string? ManagerEmail { get; set; }
    public string? GeographicLocation { get; set; }
    public string? Address { get; set; }
    public string? PreferredLanguage { get; set; }
    public List<string>? CompetenceAreas { get; set; }
    public List<string>? TechnicalSkills { get; set; }
    public string? ContractNumber { get; set; }
    public DateTime? ContractStartDate { get; set; }
    public DateTime? ContractEndDate { get; set; }
    public string? VendorId { get; set; }
    public bool IsActive { get; set; }
}

public class ActorSearchDTO
{
    public string? SearchTerm { get; set; }
    public ActorType? ActorType { get; set; }
    public SecurityClearance? SecurityClearance { get; set; }
    public string? OrganizationName { get; set; }
    public string? Department { get; set; }
    public string? GeographicLocation { get; set; }
    public bool? IsActive { get; set; } = true;
    public List<string>? CompetenceAreas { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ActorListDTO
{
    public List<ActorDTO> Actors { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class RoleAssignmentDTO
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? RoleDescription { get; set; }
    public DateTime AssignedAt { get; set; }
    public string AssignedByUserName { get; set; } = string.Empty;
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}

public class AssignRoleToActorDTO
{
    public int RoleId { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string? Notes { get; set; }
}

public class ActorNoteDTO
{
    public int Id { get; set; }
    public string Note { get; set; } = string.Empty;
    public string? Category { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public bool IsPrivate { get; set; }
}

public class CreateActorNoteDTO
{
    public string Note { get; set; } = string.Empty;
    public string? Category { get; set; }
    public bool IsPrivate { get; set; } = false;
}