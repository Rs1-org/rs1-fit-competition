using Competition.Application.Common.Models;
using Competition.Domain.Entities.MatchEntities;

namespace Competition.Application.Interfaces;

public interface IMatch : IRepository<Match>
{
    Task<Match?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    /// <summary>Includes soft-deleted matches: they still own their unique slug.</summary>
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);
    Task<Match?> GetByIdWithNavAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<Match>> GetPagedWithFiltersAsync(
        int page, int pageSize,
        Guid? statusId, Guid? stadiumId, Guid? roundId,
        bool? isPostponed,
        DateTime? dateFrom, DateTime? dateTo,
        CancellationToken cancellationToken = default);
}
