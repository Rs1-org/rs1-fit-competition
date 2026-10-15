using Competition.Application.Common.Models;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.Person;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Extensions;
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

    public Task<PagedResult<Manager>> GetPagedAsync(StaffListFilter filter, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = context.Managers
            .Where(m => m.DeletedAt == null)
            .FilterByName(filter.Name)
            .FilterByAge(filter.AgeMin, filter.AgeMax);

        if (filter.CareerStartFrom is not null)
            query = query.Where(m => m.CareerStart != null && m.CareerStart >= filter.CareerStartFrom);
        if (filter.CareerStartTo is not null)
            query = query.Where(m => m.CareerStart != null && m.CareerStart <= filter.CareerStartTo);

        return query
            .OrderBy(m => m.LastName)
            .ThenBy(m => m.FirstName)
            .ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public Task AddAsync(Manager manager, CancellationToken cancellationToken = default) =>
        context.Managers.AddAsync(manager, cancellationToken).AsTask();

    public void Remove(Manager manager) => context.Managers.Remove(manager);
}
