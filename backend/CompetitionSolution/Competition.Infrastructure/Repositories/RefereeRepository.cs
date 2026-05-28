using Competition.Application.Interfaces;
using Competition.Domain.Entities.Person;
using Competition.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Competition.Infrastructure.Repositories;

public class RefereeRepository(AppDbContext context) : IReferee
{
    public Task<Referee?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Referees
            .Where(r => r.DeletedAt == null)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<List<Referee>> GetAllActiveAsync(CancellationToken cancellationToken = default) =>
        context.Referees
            .Where(r => r.DeletedAt == null)
            .ToListAsync(cancellationToken);

    public Task AddAsync(Referee referee, CancellationToken cancellationToken = default) =>
        context.Referees.AddAsync(referee, cancellationToken).AsTask();

    public void Remove(Referee referee) => context.Referees.Remove(referee);
}
