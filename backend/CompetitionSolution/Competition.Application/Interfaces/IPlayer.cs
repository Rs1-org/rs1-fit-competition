using Competition.Application.Common.Models;
using Competition.Domain.Entities.Person;

namespace Competition.Application.Interfaces;

public interface IPlayer
{
    Task<Player?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Player>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<Player>> GetPagedAsync(PlayerListFilter filter, int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(Player player, CancellationToken cancellationToken = default);
    void Remove(Player player);
}
