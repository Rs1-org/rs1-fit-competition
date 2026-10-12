using Competition.Application.Common.Models;
using Competition.Domain.Entities.MatchEntities;

namespace Competition.Application.Interfaces;

public interface IStadium : IRepository<Stadium>
{
    Task<Stadium?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<PagedResult<Stadium>> GetPagedFilteredAsync(StadiumListFilter filter, int page, int pageSize, CancellationToken cancellationToken = default);
}
