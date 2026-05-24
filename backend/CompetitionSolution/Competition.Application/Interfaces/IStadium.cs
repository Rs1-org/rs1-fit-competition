using Competition.Domain.Entities.MatchEntities;

namespace Competition.Application.Interfaces;

public interface IStadium : IRepository<Stadium>
{
    Task<Stadium?> GetBySlugAsync(string slug);
}
