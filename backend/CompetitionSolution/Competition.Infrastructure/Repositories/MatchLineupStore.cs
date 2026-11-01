using Competition.Application.Interfaces;
using Competition.Domain.Entities.MatchEntities;
using Competition.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Competition.Infrastructure.Repositories;

public class MatchLineupStore(AppDbContext db) : IMatchLineup
{
    public Task<Match?> GetMatchAsync(Guid matchId, CancellationToken cancellationToken = default) =>
        db.Matches
            .Include(m => m.HomeSelection)
            .Include(m => m.AwaySelection)
            .Include(m => m.Referee)
            .Include(m => m.HomeFormation)
            .Include(m => m.AwayFormation)
            .Include(m => m.Lineups).ThenInclude(l => l.Player)
            .Include(m => m.Lineups).ThenInclude(l => l.Selection)
            .Include(m => m.Lineups).ThenInclude(l => l.Position)
            .Include(m => m.Lineups).ThenInclude(l => l.SubPosition)
            .FirstOrDefaultAsync(m => m.Id == matchId, cancellationToken);

    public Task<bool> PlayerInSelectionAsync(Guid playerId, Guid selectionId, CancellationToken cancellationToken = default) =>
        db.PlayerSelectionHistories.AnyAsync(
            h => h.DeletedAt == null && h.PlayerId == playerId && h.SelectionId == selectionId,
            cancellationToken);

    public void Remove(MatchLineup lineup) => db.Lineups.Remove(lineup);

    public Task AddAsync(MatchLineup lineup, CancellationToken cancellationToken = default) =>
        db.Lineups.AddAsync(lineup, cancellationToken).AsTask();
}
