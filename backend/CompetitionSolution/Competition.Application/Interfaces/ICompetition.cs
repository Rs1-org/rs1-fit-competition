using Competition.Application.Common.Models;
using CompetitionEntity = Competition.Domain.Entities.CompetitionEntities.Competition;

namespace Competition.Application.Interfaces;

public interface ICompetition : IRepository<CompetitionEntity>
{
    Task<CompetitionEntity?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<PagedResult<CompetitionEntity>> GetPagedWithFiltersAsync(
        int page, int pageSize,
        string? search, Guid? countryId,
        CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId, CancellationToken cancellationToken = default);
    Task<bool> HasSeasonsAsync(Guid id, CancellationToken cancellationToken = default);
}
