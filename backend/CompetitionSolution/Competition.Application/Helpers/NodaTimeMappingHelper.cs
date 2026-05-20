using NodaTime;
using NodaTime.Text;

namespace Competition.Application.Helpers;

public static class NodaTimeMappingHelper
{
    private static readonly LocalTimePattern TimePattern = LocalTimePattern.CreateWithInvariantCulture("HH:mm:ss");

    public static LocalTime ParseLocalTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return LocalTime.Midnight;

        if (TimeOnly.TryParse(value, out var timeOnly))
            return LocalTime.FromTimeOnly(timeOnly);

        var parsed = TimePattern.Parse(value);
        return parsed.Success ? parsed.Value : LocalTime.Midnight;
    }

    public static LocalTime? ParseNullableLocalTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (TimeOnly.TryParse(value, out var timeOnly))
            return LocalTime.FromTimeOnly(timeOnly);

        var parsed = TimePattern.Parse(value);
        return parsed.Success ? parsed.Value : null;
    }

    public static string FormatLocalTime(LocalTime value) =>
        value.ToString("HH:mm:ss", null);

    public static string? FormatNullableLocalTime(LocalTime? value) =>
        value.HasValue ? FormatLocalTime(value.Value) : null;
}
