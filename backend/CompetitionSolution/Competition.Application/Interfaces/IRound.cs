using Competition.Application.Common.Models;
using Competition.Domain.Entities.CompetitionEntities;

namespace Competition.Application.Interfaces;

public interface IRound : IRepository<Round>
{
    Task<Round?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<PagedResult<Round>> GetPagedWithFiltersAsync(
        int page, int pageSize,
        Guid? seasonId, string? search,
        CancellationToken cancellationToken = default);
    Task<bool> NumberExistsInSeasonAsync(Guid seasonId, int number, Guid? excludeId, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId, CancellationToken cancellationToken = default);
    Task<bool> HasMatchesAsync(Guid id, CancellationToken cancellationToken = default);
}
