using Competition.Application.Common.Models;
using Competition.Domain.Entities.Person;

namespace Competition.Application.Interfaces;

public interface IManager
{
    Task<Manager?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Manager>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<Manager>> GetPagedAsync(StaffListFilter filter, int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(Manager manager, CancellationToken cancellationToken = default);
    void Remove(Manager manager);
}
