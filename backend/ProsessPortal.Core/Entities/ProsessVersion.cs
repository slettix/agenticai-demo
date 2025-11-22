namespace ProsessPortal.Core.Entities;

public class ProsessVersion
{
    public int Id { get; set; }
    public int ProsessId { get; set; }
    public Prosess Prosess { get; set; } = null!;
    public string VersionNumber { get; set; } = string.Empty; // e.g., "1.0.0", "1.1.0"
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty; // Markdown/HTML content
    public string ChangeLog { get; set; } = string.Empty;
    public string? GitCommitHash { get; set; }
    public string? GitTag { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;
    public bool IsCurrent { get; set; } = false; // Only one version should be current
    public bool IsPublished { get; set; } = false;
    public DateTime? PublishedAt { get; set; }
    public int? PublishedByUserId { get; set; }
    public User? PublishedByUser { get; set; }
}

public static class VersionHelper
{
    public static string GetNextVersion(string currentVersion, VersionChangeType changeType)
    {
        var parts = currentVersion.Split('.').Select(int.Parse).ToArray();
        if (parts.Length != 3) return "1.0.0";

        return changeType switch
        {
            VersionChangeType.Major => $"{parts[0] + 1}.0.0",
            VersionChangeType.Minor => $"{parts[0]}.{parts[1] + 1}.0",
            VersionChangeType.Patch => $"{parts[0]}.{parts[1]}.{parts[2] + 1}",
            _ => currentVersion
        };
    }
}

public enum VersionChangeType
{
    Patch = 0,  // Bug fixes, minor corrections
    Minor = 1,  // New features, non-breaking changes
    Major = 2   // Breaking changes, major revisions
}