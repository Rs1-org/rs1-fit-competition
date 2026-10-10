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

    public Task<PagedResult<Selection>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default)
    {
        var query = _context.Selections.Include(x => x.Country).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(x =>
                x.Name.ToLower().Contains(term)
                || x.Abbreviation.ToLower().Contains(term)
                || (x.Country != null && x.Country.Name != null && x.Country.Name.ToLower().Contains(term)));
        }

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

        var mandats = await _context.MandatDates
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
                if (!SelectionAppearanceRules.CoversDate(matchDate, mandat?.StartMandatDate, mandat?.EndMandatDate))
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
