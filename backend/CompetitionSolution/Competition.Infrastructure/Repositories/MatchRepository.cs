using Competition.Application.Interfaces;
using Competition.Domain.Entities.MatchEntities;
using Competition.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Competition.Infrastructure.Repositories;

public class MatchRepository : Repository<Match>, IMatch
{
    private readonly AppDbContext _context;

    public MatchRepository(AppDbContext context) : base(context)
        => _context = context;

    public async Task<Match?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        await _context.Matches
            .Include(m => m.Status)
            .Include(m => m.Stadium)
            .FirstOrDefaultAsync(m => m.Slug == slug, cancellationToken);
}
