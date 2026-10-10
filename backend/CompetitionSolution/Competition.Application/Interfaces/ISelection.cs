using Competition.Application.Common.Models;
using Competition.Application.DTOs.SelectionDto;
using Competition.Domain.Entities;

namespace Competition.Application.Interfaces;

public interface ISelection : IRepository<Selection>
{
    Task<Selection?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<PagedResult<Selection>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default);
}

public interface IPlayerAppearances
{
    Task<IReadOnlyList<PlayerAppearanceDto>> ForPlayerAsync(Guid playerId, CancellationToken cancellationToken = default);
}
