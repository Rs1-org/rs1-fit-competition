using Competition.Application.Common.Models;
using Competition.Application.DTOs.SelectionDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Competition.Infrastructure.Repositories;

public class SelectionRepository : Repository<Selection>, ISelection
{
    private readonly AppDbContext _context;

    public SelectionRepository(AppDbContext context) : base(context) => _context = context;

    public Task<Selection?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        _context.Selections.Include(x => x.Country).FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);

    public Task<PagedResult<Selection>> GetPagedAsync(SelectionListFilter filter, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Selections.Include(x => x.Country).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            var name = filter.Name.Trim().ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(filter.Abbreviation))
        {
            var abbreviation = filter.Abbreviation.Trim().ToLower();
            query = query.Where(x => x.Abbreviation.ToLower().Contains(abbreviation));
        }

        if (!string.IsNullOrWhiteSpace(filter.CountryPublicId))
        {
            var countryPublicId = filter.CountryPublicId.Trim();
            query = query.Where(x => x.Country != null && x.Country.PublicId == countryPublicId);
        }

        if (filter.FollowersMin.HasValue)
            query = query.Where(x => x.FollowersCount >= filter.FollowersMin.Value);

        if (filter.FollowersMax.HasValue)
            query = query.Where(x => x.FollowersCount <= filter.FollowersMax.Value);

        return query.OrderBy(x => x.Name).ToPagedResultAsync(page, pageSize, cancellationToken);
    }
}

public class PlayerAppearanceReader : IPlayerAppearances
{
    private readonly AppDbContext _context;

    public PlayerAppearanceReader(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<PlayerAppearanceDto>> ForPlayerAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        var histories = await _context.PlayerSelectionHistories
            .Include(h => h.Selection)
            .Where(h => h.DeletedAt == null && h.PlayerId == playerId && h.SelectionId != null)
            .ToListAsync(cancellationToken);

        if (histories.Count == 0)
            return [];

        var historyIds = histories.Select(h => h.Id).ToList();
        var selectionIds = histories.Select(h => h.SelectionId!.Value).Distinct().ToList();

        var mandats = await _context.MandateDates
            .Where(m => m.DeletedAt == null && m.PlayerSelectionHistoryId != null && historyIds.Contains(m.PlayerSelectionHistoryId.Value))
            .ToListAsync(cancellationToken);

        var matches = await _context.Matches
            .Include(m => m.Stadium)
            .Where(m =>
                (m.HomeSelectionId != null && selectionIds.Contains(m.HomeSelectionId.Value))
                || (m.AwaySelectionId != null && selectionIds.Contains(m.AwaySelectionId.Value)))
            .ToListAsync(cancellationToken);

        var seen = new HashSet<(Guid MatchId, Guid SelectionId)>();
        var result = new List<PlayerAppearanceDto>();

        foreach (var history in histories)
        {
            var mandat = mandats.FirstOrDefault(m => m.PlayerSelectionHistoryId == history.Id);
            foreach (var match in matches)
            {
                var side = SelectionAppearanceRules.Side(history.SelectionId!.Value, match.HomeSelectionId, match.AwaySelectionId);
                if (side is null)
                    continue;

                var matchDate = match.Date.InUtc().Date;
                if (!SelectionAppearanceRules.CoversDate(matchDate, mandat?.StartDate, mandat?.EndDate))
                    continue;

                if (!seen.Add((match.Id, history.SelectionId.Value)))
                    continue;

                result.Add(new PlayerAppearanceDto
                {
                    MatchId = match.Id,
                    MatchSlug = match.Slug,
                    MatchDate = matchDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                    StadiumName = match.Stadium?.Name,
                    SelectionId = history.SelectionId.Value,
                    SelectionName = history.Selection?.Name ?? string.Empty,
                    Side = side,
                    ShirtNumber = history.ShirtNumber
                });
            }
        }

        return result.OrderBy(x => x.MatchDate).ThenBy(x => x.SelectionName).ToList();
    }
}
