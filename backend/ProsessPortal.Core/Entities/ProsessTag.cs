namespace ProsessPortal.Core.Entities;

public class ProsessTag
{
    public int Id { get; set; }
    public int ProsessId { get; set; }
    public Prosess Prosess { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#007bff"; // Hex color for UI display
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// Predefined categories and tags for consistent organization
public static class ProsessCategories
{
    public const string HR = "HR";
    public const string IT = "IT";
    public const string Finance = "Økonomi";
    public const string Operations = "Drift";
    public const string Quality = "Kvalitet";
    public const string Compliance = "Compliance";
    public const string Marketing = "Markedsføring";
    public const string Sales = "Salg";
    public const string CustomerService = "Kundeservice";
    public const string General = "Generell";
}

public static class CommonTags
{
    public const string Critical = "Kritisk";
    public const string Automated = "Automatisert";
    public const string Manual = "Manuell";
    public const string QuickWin = "Quick Win";
    public const string Complex = "Kompleks";
    public const string Frequent = "Hyppig";
    public const string Rare = "Sjelden";
    public const string CustomerFacing = "Kundevendt";
    public const string Internal = "Intern";
    public const string Regulatory = "Regulatorisk";
}