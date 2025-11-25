namespace ProsessPortal.Core.Entities;

public class ProsessDeletionHistory
{
    public int Id { get; set; }
    public int ProsessId { get; set; }
    public Prosess Prosess { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string Action { get; set; } = string.Empty; // "DELETE", "RESTORE", "HARD_DELETE"
    public string? Reason { get; set; }
    public DateTime ActionAt { get; set; } = DateTime.UtcNow;
}

public static class DeletionActions
{
    public const string SoftDelete = "SOFT_DELETE";
    public const string HardDelete = "HARD_DELETE";
    public const string Restore = "RESTORE";
    public const string BulkDelete = "BULK_DELETE";
}