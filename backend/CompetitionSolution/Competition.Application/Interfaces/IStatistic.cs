using Competition.Application.Common.Models;
using Competition.Domain.Entities.MatchEntities;
using NodaTime;

namespace Competition.Application.Interfaces;

public record StatisticFilter(string? MatchPublicId, string? SelectionPublicId,
    Instant? DateFrom, Instant? DateTo, int? MinTotalShots);

public interface IStatistic : IRepository<Statistic>
{
    Task<Statistic?> GetDetailsAsync(string publicId, CancellationToken cancellationToken = default);
    Task<Statistic?> GetForMatchAsync(string matchPublicId, CancellationToken cancellationToken = default);
    Task<bool> ExistsForMatchAsync(Guid matchId, CancellationToken cancellationToken = default);
    Task<PagedResult<Statistic>> GetFilteredAsync(StatisticFilter filter, int page, int pageSize,
        CancellationToken cancellationToken = default);
}
