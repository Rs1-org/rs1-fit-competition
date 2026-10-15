using Competition.Application.Common.Models;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.Person;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Extensions;
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

    public Task<PagedResult<Referee>> GetPagedAsync(StaffListFilter filter, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = context.Referees
            .Where(r => r.DeletedAt == null)
            .FilterByName(filter.Name)
            .FilterByAge(filter.AgeMin, filter.AgeMax);

        if (filter.CareerStartFrom is not null)
            query = query.Where(r => r.CareerStart != null && r.CareerStart >= filter.CareerStartFrom);
        if (filter.CareerStartTo is not null)
            query = query.Where(r => r.CareerStart != null && r.CareerStart <= filter.CareerStartTo);

        return query
            .OrderBy(r => r.LastName)
            .ThenBy(r => r.FirstName)
            .ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public Task AddAsync(Referee referee, CancellationToken cancellationToken = default) =>
        context.Referees.AddAsync(referee, cancellationToken).AsTask();

    public void Remove(Referee referee) => context.Referees.Remove(referee);
}
