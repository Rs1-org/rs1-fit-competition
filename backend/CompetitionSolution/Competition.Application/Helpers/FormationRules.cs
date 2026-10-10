namespace Competition.Application.Helpers;

public static class FormationRules
{
    public static bool CodeMatchesMark(string? code, int defenders, int midfielders, int attackers)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        var parts = code.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 3)
            return false;

        return int.TryParse(parts[0], out var parsedDefenders)
            && int.TryParse(parts[1], out var parsedMidfielders)
            && int.TryParse(parts[2], out var parsedAttackers)
            && parsedDefenders == defenders
            && parsedMidfielders == midfielders
            && parsedAttackers == attackers;
    }
}
