namespace Competition.Application.Common.Models;

/// <summary>
/// Column sorting for list endpoints (RS1 item 13). <see cref="SortBy"/> is one of the
/// entity's <see cref="SortKeys"/>; anything else is rejected by the validators.
/// </summary>
public record SortSpec(string? SortBy, string? SortDir)
{
    public bool Descending => string.Equals(SortDir, "desc", StringComparison.OrdinalIgnoreCase);

    public bool Is(string key) => string.Equals(SortBy, key, StringComparison.OrdinalIgnoreCase);
}

/// <summary>Sortable columns per list (camelCase, as the frontend sends them).</summary>
public static class SortKeys
{
    public static readonly string[] Country = ["name", "createdAt"];
    public static readonly string[] City = ["name", "population", "country"];
    public static readonly string[] Stadium = ["name", "capacity", "city", "dateOfConstruction"];
}
