using NodaTime;
using NodaTime.Text;

namespace Competition.Application.Helpers;

public static class NodaTimeMappingHelper
{
    private static readonly LocalDatePattern DatePattern = LocalDatePattern.CreateWithInvariantCulture("yyyy-MM-dd");

    public static LocalDate ParseLocalDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new LocalDate(2000, 1, 1);

        if (DateOnly.TryParse(value, out var dateOnly))
            return LocalDate.FromDateOnly(dateOnly);

        var parsed = DatePattern.Parse(value);
        return parsed.Success ? parsed.Value : new LocalDate(2000, 1, 1);
    }

    public static LocalDate? ParseNullableLocalDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (DateOnly.TryParse(value, out var dateOnly))
            return LocalDate.FromDateOnly(dateOnly);

        var parsed = DatePattern.Parse(value);
        return parsed.Success ? parsed.Value : null;
    }

    public static string FormatLocalDate(LocalDate value) =>
        value.ToString("yyyy-MM-dd", null);

    public static string? FormatNullableLocalDate(LocalDate? value) =>
        value.HasValue ? FormatLocalDate(value.Value) : null;
}
