using Competition.Application.Interfaces;
using Competition.Domain.Entities.Person;
using Competition.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Competition.Infrastructure.Repositories;

public class PlayerRepository(AppDbContext context) : IPlayer
{
    public Task<Player?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Players
            .Where(p => p.DeletedAt == null)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<List<Player>> GetAllActiveAsync(CancellationToken cancellationToken = default) =>
        context.Players
            .Where(p => p.DeletedAt == null)
            .ToListAsync(cancellationToken);

    public Task AddAsync(Player player, CancellationToken cancellationToken = default) =>
        context.Players.AddAsync(player, cancellationToken).AsTask();

    public void Remove(Player player) => context.Players.Remove(player);
}
