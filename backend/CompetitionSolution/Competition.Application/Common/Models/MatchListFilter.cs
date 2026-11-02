namespace Competition.Application.Common.Models;

public record MatchListFilter(
    Guid? StatusId = null,
    Guid? StadiumId = null,
    Guid? RoundId = null,
    bool? IsPostponed = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null,
    Guid? SelectionId = null,
    Guid? CompetitionId = null,
    string? Phase = null,
    SortSpec? Sort = null);

/// <summary>Quick tabs of the match list. A phase can cover several statuses, unlike the status filter.</summary>
public static class MatchPhases
{
    public const string Live = "live";
    public const string Results = "results";
    public const string Upcoming = "upcoming";

    public static readonly string[] All = [Live, Results, Upcoming];

    /// <summary>Lowercase status names of a phase; postponed matches belong to no phase.</summary>
    public static string[] StatusNames(string phase) => phase.ToLowerInvariant() switch
    {
        Live => ["live", "halftime", "half-time"],
        Results => ["finished"],
        Upcoming => ["scheduled"],
        _ => []
    };
}
