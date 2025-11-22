namespace ProsessPortal.Core.Entities;

public class Permission
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public class RolePermission
{
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
    
    public int PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
}

public static class PermissionNames
{
    // Prosess permissions
    public const string ViewProsess = "view_prosess";
    public const string CreateProsess = "create_prosess";
    public const string EditProsess = "edit_prosess";
    public const string DeleteProsess = "delete_prosess";
    public const string ApproveProsess = "approve_prosess";
    
    // QA permissions
    public const string ViewQAQueue = "view_qa_queue";
    public const string ApproveChanges = "approve_changes";
    public const string RejectChanges = "reject_changes";
    
    // Admin permissions
    public const string ManageUsers = "manage_users";
    public const string ManageRoles = "manage_roles";
    public const string ViewAuditLog = "view_audit_log";
}