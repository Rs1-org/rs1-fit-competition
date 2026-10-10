using NodaTime;

namespace Competition.Application.Helpers;

public static class SelectionAppearanceRules
{
    public static bool CoversDate(LocalDate matchDate, LocalDate? start, LocalDate? end)
    {
        if (start is null || matchDate < start.Value)
            return false;

        return end is null || matchDate <= end.Value;
    }

    public static string? Side(Guid selectionId, Guid? homeSelectionId, Guid? awaySelectionId)
    {
        if (homeSelectionId == selectionId)
            return "home";

        if (awaySelectionId == selectionId)
            return "away";

        return null;
    }
}
