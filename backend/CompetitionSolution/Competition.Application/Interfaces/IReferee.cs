using Competition.Domain.Entities.Person;

namespace Competition.Application.Interfaces;

public interface IReferee
{
    Task<Referee?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Referee>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Referee referee, CancellationToken cancellationToken = default);
    void Remove(Referee referee);
}
