using Competition.Application.Common.Models;
using Competition.Application.DTOs.SelectionDto;
using Competition.Domain.Entities;

namespace Competition.Application.Interfaces;

public record SelectionListFilter(
    string? Name,
    string? CountryPublicId,
    string? Abbreviation,
    int? FollowersMin,
    int? FollowersMax,
    string? ClubPublicId = null);

public interface ISelection : IRepository<Selection>
{
    Task<Selection?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<PagedResult<Selection>> GetPagedAsync(SelectionListFilter filter, int page, int pageSize, CancellationToken cancellationToken = default);
}

public interface IPlayerAppearances
{
    Task<IReadOnlyList<PlayerAppearanceDto>> ForPlayerAsync(Guid playerId, CancellationToken cancellationToken = default);
}
