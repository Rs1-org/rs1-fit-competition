using Competition.Application.Common.Models;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.MatchEntities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Competition.Infrastructure.Repositories;

public class MatchStatusRepository : Repository<MatchStatus>, IMatchStatus
{
    private readonly AppDbContext _context;

    public MatchStatusRepository(AppDbContext context) : base(context)
        => _context = context;

    public async Task<PagedResult<MatchStatus>> GetPagedOrderedAsync(int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.MatchStatuses
            .AsNoTracking()
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Name)
            .ToPagedResultAsync(page, pageSize, cancellationToken);
}
