using Competition.Application.Common.Models;
using Competition.Domain.Entities.MatchEntities;

namespace Competition.Application.Interfaces;

public interface IMatchStatus : IRepository<MatchStatus>
{
    Task<PagedResult<MatchStatus>> GetPagedOrderedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}
