namespace Competition.Infrastructure.Settings;

public class SeedSettings
{
    public const string SectionName = "Seed";

    public bool Enabled { get; set; }

    public string SuperAdminPassword { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
    public string ModeratorPassword { get; set; } = string.Empty;
    public string ManagerPassword { get; set; } = string.Empty;
    public string UserPassword { get; set; } = string.Empty;
}
