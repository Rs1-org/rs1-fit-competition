namespace Competition.Infrastructure.Settings;

public class CorsSettings
{
    public const string SectionName = "Cors";
    public const string PolicyName = "ConfiguredOrigins";

    public string[] AllowedOrigins { get; set; } = [];
}
