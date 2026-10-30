using Competition.Application.Common.Models;
using Competition.Domain.Entities.MatchEntities;
using NodaTime;

namespace Competition.Application.Interfaces;

public record StatisticFilter(string? MatchPublicId, string? SelectionPublicId,
    Instant? DateFrom, Instant? DateTo, int? MinTotalShots);

public record ChartFilter(string? CompetitionPublicId, string? SeasonPublicId);

/// <summary>One finished match with both scores set, flattened for chart aggregation.</summary>
public record ChartMatchRow(int? RoundNumber, string? RoundName,
    string HomePublicId, string HomeName, int HomeScore,
    string AwayPublicId, string AwayName, int AwayScore);

public record ChartTeamShotRow(string SelectionPublicId, string SelectionName,
    int TotalShots, int ShotsOnTarget, int Possession);

/// <summary>One finished match with both scores set, newest first in advanced statistics.</summary>
public record AdvancedMatchRow(Instant Date, string HomePublicId, string HomeName, int HomeScore,
    string AwayPublicId, string AwayName, int AwayScore);

public record PlayerStatsRow(string PlayerPublicId, string PlayerName, string SelectionName,
    int Goals, int YellowCards, int RedCards);

public interface IStatistic : IRepository<Statistic>
{
    Task<IReadOnlyList<AdvancedMatchRow>> GetTeamMatchesAsync(string selectionPublicId, int last, ChartFilter filter,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdvancedMatchRow>> GetHeadToHeadMatchesAsync(string firstPublicId, string secondPublicId, ChartFilter filter,
        CancellationToken cancellationToken = default);
    Task<PagedResult<PlayerStatsRow>> GetPlayerStatsAsync(string? selectionPublicId, ChartFilter filter, int page, int pageSize,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChartMatchRow>> GetChartMatchesAsync(ChartFilter filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChartTeamShotRow>> GetChartTeamShotsAsync(ChartFilter filter, CancellationToken cancellationToken = default);
    Task<Statistic?> GetDetailsAsync(string publicId, CancellationToken cancellationToken = default);
    Task<Statistic?> GetForMatchAsync(string matchPublicId, CancellationToken cancellationToken = default);
    Task<bool> ExistsForMatchAsync(Guid matchId, CancellationToken cancellationToken = default);
    Task<PagedResult<Statistic>> GetFilteredAsync(StatisticFilter filter, int page, int pageSize,
        CancellationToken cancellationToken = default);
}
