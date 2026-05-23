using Competition.Domain.Entities;

namespace Competition.Application.Interfaces;

public interface IClubRepository : IRepository<Club>
{
    Task<Club?> GetBySlugAsync(string slug);
    Task<Club?> GetDetailedByIdAsync(Guid id);
}
