using Competition.Domain.Entities.MatchEntities;

namespace Competition.Application.Interfaces;

public interface IMatchLineup
{
    Task<Match?> GetMatchAsync(Guid matchId, CancellationToken cancellationToken = default);
    Task<bool> PlayerInSelectionAsync(Guid playerId, Guid selectionId, CancellationToken cancellationToken = default);
    void Remove(MatchLineup lineup);
    Task AddAsync(MatchLineup lineup, CancellationToken cancellationToken = default);
}
