using Competition.Application.Interfaces;
using Competition.Domain.Entities.Person;
using Competition.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Competition.Infrastructure.Repositories;

public class ManagerRepository(AppDbContext context) : IManager
{
    public Task<Manager?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Managers
            .Where(m => m.DeletedAt == null)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public Task<List<Manager>> GetAllActiveAsync(CancellationToken cancellationToken = default) =>
        context.Managers
            .Where(m => m.DeletedAt == null)
            .ToListAsync(cancellationToken);

    public Task AddAsync(Manager manager, CancellationToken cancellationToken = default) =>
        context.Managers.AddAsync(manager, cancellationToken).AsTask();

    public void Remove(Manager manager) => context.Managers.Remove(manager);
}
