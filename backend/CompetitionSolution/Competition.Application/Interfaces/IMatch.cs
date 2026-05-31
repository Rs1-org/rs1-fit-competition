using Competition.Domain.Entities.MatchEntities;

namespace Competition.Application.Interfaces;

public interface IMatch : IRepository<Match>
{
    Task<Match?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
}
