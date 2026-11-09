namespace Competition.Infrastructure.Settings;

public class ReportSettings
{
    public const string SectionName = "Reports";

    /// <summary>IANA time zone used for times printed in PDF reports. Unknown ids fall back to UTC.</summary>
    public string TimeZone { get; set; } = "Europe/Sarajevo";
}
