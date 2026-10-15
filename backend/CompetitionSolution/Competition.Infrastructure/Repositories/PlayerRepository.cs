using Competition.Application.Common.Models;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.Person;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Competition.Infrastructure.Repositories;

public class PlayerRepository(AppDbContext context) : IPlayer
{
    public Task<Player?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Players
            .Include(p => p.Position)
            .Include(p => p.SubPosition)
            .Where(p => p.DeletedAt == null)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<List<Player>> GetAllActiveAsync(CancellationToken cancellationToken = default) =>
        context.Players
            .Include(p => p.Position)
            .Include(p => p.SubPosition)
            .Where(p => p.DeletedAt == null)
            .ToListAsync(cancellationToken);

    public Task<PagedResult<Player>> GetPagedAsync(PlayerListFilter filter, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = context.Players
            .Include(p => p.Position)
            .Include(p => p.SubPosition)
            .Where(p => p.DeletedAt == null)
            .FilterByName(filter.Name)
            .FilterByAge(filter.AgeMin, filter.AgeMax);

        if (filter.PositionId.HasValue)
            query = query.Where(p => p.PositionId == filter.PositionId);
        if (filter.SubPositionId.HasValue)
            query = query.Where(p => p.SubPositionId == filter.SubPositionId);

        return query
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public Task AddAsync(Player player, CancellationToken cancellationToken = default) =>
        context.Players.AddAsync(player, cancellationToken).AsTask();

    public void Remove(Player player) => context.Players.Remove(player);
}
