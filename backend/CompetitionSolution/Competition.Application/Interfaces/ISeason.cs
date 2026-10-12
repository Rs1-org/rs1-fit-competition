using Competition.Application.Common.Models;
using Competition.Domain.Entities.CompetitionEntities;

namespace Competition.Application.Interfaces;

public interface ISeason : IRepository<Season>
{
    Task<Season?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<PagedResult<Season>> GetPagedWithFiltersAsync(
        int page, int pageSize,
        Guid? competitionId, bool? isCurrent, string? search,
        CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId, CancellationToken cancellationToken = default);
    Task<bool> HasRoundsAsync(Guid id, CancellationToken cancellationToken = default);
    Task UnsetCurrentAsync(Guid competitionId, Guid? exceptId, CancellationToken cancellationToken = default);
}
