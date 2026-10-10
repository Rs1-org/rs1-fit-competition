using Competition.Application.Common.Models;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.FormationEntities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Competition.Infrastructure.Repositories;

public class FormationMarkRepository : Repository<FormationMark>, IFormationMark
{
    private readonly AppDbContext _context;

    public FormationMarkRepository(AppDbContext context) : base(context) => _context = context;

    public Task<FormationMark?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        _context.FormationMarks.FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);

    public Task<PagedResult<FormationMark>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default)
    {
        var query = _context.FormationMarks.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                (x.Slug != null && x.Slug.Contains(term))
                || x.Defenders.ToString().Contains(term)
                || x.Midfielders.ToString().Contains(term)
                || x.Attackers.ToString().Contains(term));
        }

        return query.OrderBy(x => x.Defenders).ThenBy(x => x.Midfielders).ToPagedResultAsync(page, pageSize, cancellationToken);
    }
}

public class FormationRepository : Repository<Formation>, IFormation
{
    private readonly AppDbContext _context;

    public FormationRepository(AppDbContext context) : base(context) => _context = context;

    public Task<Formation?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        _context.Formations.Include(x => x.FormationMark).FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);

    public Task<Formation?> GetByMarkIdAsync(Guid formationMarkId, CancellationToken cancellationToken = default) =>
        _context.Formations.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.FormationMarkId == formationMarkId, cancellationToken);

    public Task<PagedResult<Formation>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default)
    {
        var query = _context.Formations.Include(x => x.FormationMark).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(x => x.Code.ToLower().Contains(term) || (x.Slug != null && x.Slug.Contains(term)));
        }

        return query.OrderBy(x => x.Code).ToPagedResultAsync(page, pageSize, cancellationToken);
    }
}

public class PositionRepository : Repository<Position>, IPosition
{
    private readonly AppDbContext _context;

    public PositionRepository(AppDbContext context) : base(context) => _context = context;

    public Task<Position?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        _context.Positions.FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);

    public Task<bool> HasSubPositionsAsync(Guid positionId, CancellationToken cancellationToken = default) =>
        _context.SubPositions.AnyAsync(x => x.PositionId == positionId, cancellationToken);

    public Task<bool> HasPlayersAsync(Guid positionId, CancellationToken cancellationToken = default) =>
        _context.Players.AnyAsync(x => x.PositionId == positionId && x.DeletedAt == null, cancellationToken);

    public Task<PagedResult<Position>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default)
    {
        var query = _context.Positions.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(term) || (x.Slug != null && x.Slug.Contains(term)));
        }

        return query.OrderBy(x => x.Name).ToPagedResultAsync(page, pageSize, cancellationToken);
    }
}

public class SubPositionRepository : Repository<SubPosition>, ISubPosition
{
    private readonly AppDbContext _context;

    public SubPositionRepository(AppDbContext context) : base(context) => _context = context;

    public Task<SubPosition?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        _context.SubPositions.Include(x => x.Position).FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);

    public Task<SubPosition?> GetWithPositionAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.SubPositions.Include(x => x.Position).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<bool> HasPlayersAsync(Guid subPositionId, CancellationToken cancellationToken = default) =>
        _context.Players.AnyAsync(x => x.SubPositionId == subPositionId && x.DeletedAt == null, cancellationToken);

    public Task<PagedResult<SubPosition>> GetPagedAsync(int page, int pageSize, string? search, Guid? positionId, CancellationToken cancellationToken = default)
    {
        var query = _context.SubPositions.Include(x => x.Position).AsQueryable();
        if (positionId.HasValue)
            query = query.Where(x => x.PositionId == positionId.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(term) || (x.Slug != null && x.Slug.Contains(term)));
        }

        return query.OrderBy(x => x.Name).ToPagedResultAsync(page, pageSize, cancellationToken);
    }
}
