using System.Linq.Expressions;
using Competition.Application.Common.Models;
using Competition.Application.Events;
using Competition.Domain.Entities.MatchEntities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Competition.Infrastructure.Repositories;

public class EventStore(AppDbContext db) : IEventStore
{
    private static readonly Expression<Func<Event, EventDto>> Projection = x => new EventDto(
        x.PublicId, x.Match.PublicId, (x.Match.HomeSelection == null ? "Home selection not assigned" : x.Match.HomeSelection.Name)
            + " vs " + (x.Match.AwaySelection == null ? "Away selection not assigned" : x.Match.AwaySelection.Name),
        new(x.EventType.PublicId, x.EventType.Code, x.EventType.Name, x.EventType.SortOrder),
        new(x.MatchPart.PublicId, x.MatchPart.Code, x.MatchPart.Name, x.MatchPart.SortOrder),
        new(x.Selection.PublicId, x.Selection.Name),
        x.Player == null ? null : new(x.Player.PublicId, x.Player.FirstName + " " + x.Player.LastName),
        x.RelatedPlayer == null ? null : new(x.RelatedPlayer.PublicId, x.RelatedPlayer.FirstName + " " + x.RelatedPlayer.LastName),
        x.Minute, x.AddedMinute, x.Note);

    public async Task<PagedResult<EventDto>> GetPageAsync(GetEventsQuery filter, CancellationToken ct)
    {
        var query = db.Events.AsNoTracking();
        if (filter.MatchPublicId is not null) query = query.Where(x => x.Match.PublicId == filter.MatchPublicId);
        if (filter.EventTypePublicId is not null) query = query.Where(x => x.EventType.PublicId == filter.EventTypePublicId);
        if (filter.SelectionPublicId is not null) query = query.Where(x => x.Selection.PublicId == filter.SelectionPublicId);
        if (filter.PlayerPublicId is not null) query = query.Where(x => x.Player != null && x.Player.PublicId == filter.PlayerPublicId
            || x.RelatedPlayer != null && x.RelatedPlayer.PublicId == filter.PlayerPublicId);
        if (filter.MinuteFrom.HasValue) query = query.Where(x => x.Minute >= filter.MinuteFrom.Value);
        if (filter.MinuteTo.HasValue) query = query.Where(x => x.Minute <= filter.MinuteTo.Value);
        return await query.OrderByDescending(x => x.Match.Date).ThenBy(x => x.Match.PublicId)
            .ThenBy(x => x.MatchPart.SortOrder).ThenBy(x => x.Minute).ThenBy(x => x.AddedMinute)
            .ThenBy(x => x.CreatedAt).ThenBy(x => x.PublicId).Select(Projection)
            .ToPagedResultAsync(filter.Page, filter.PageSize, ct);
    }
    public Task<EventDto?> GetAsync(string publicId, CancellationToken ct) =>
        db.Events.AsNoTracking().Where(x => x.PublicId == publicId).Select(Projection).FirstOrDefaultAsync(ct);
    public Task<Event?> FindAsync(string publicId, CancellationToken ct) =>
        db.Events.Include(x => x.Match).FirstOrDefaultAsync(x => x.PublicId == publicId, ct);
    public async Task<EventLookupsDto> LookupsAsync(CancellationToken ct) => new(
        await db.EventTypes.AsNoTracking().OrderBy(x => x.SortOrder).ThenBy(x => x.PublicId)
            .Select(x => new EventLookupDto(x.PublicId, x.Code, x.Name, x.SortOrder)).ToListAsync(ct),
        await db.MatchParts.AsNoTracking().OrderBy(x => x.SortOrder).ThenBy(x => x.PublicId)
            .Select(x => new EventLookupDto(x.PublicId, x.Code, x.Name, x.SortOrder)).ToListAsync(ct));
    public async Task<EventContextDto?> ContextAsync(string matchPublicId, CancellationToken ct)
    {
        var match = await db.Matches.AsNoTracking().Where(x => x.PublicId == matchPublicId)
            .Select(x => new { x.PublicId, x.HomeSelectionId, x.AwaySelectionId }).FirstOrDefaultAsync(ct);
        if (match is null) return null;
        var selections = await db.Selections.AsNoTracking()
            .Where(x => x.Id == match.HomeSelectionId || x.Id == match.AwaySelectionId)
            .Select(x => new { x.Id, x.PublicId, x.Name }).ToListAsync(ct);
        var ids = selections.Select(x => x.Id).ToArray();
        // History has no validity dates: use active recorded memberships, without claiming a historical lineup.
        var members = await db.PlayerSelectionHistories.AsNoTracking()
            .Where(x => x.SelectionId.HasValue && ids.Contains(x.SelectionId.Value) && x.Player.DeletedAt == null)
            .Select(x => new { SelectionId = x.SelectionId!.Value, x.Player.PublicId, Name = x.Player.FirstName + " " + x.Player.LastName })
            .Distinct().OrderBy(x => x.Name).ThenBy(x => x.PublicId).ToListAsync(ct);
        return new(match.PublicId,
            selections.FirstOrDefault(x => x.Id == match.HomeSelectionId)?.Name ?? "Home selection not assigned",
            selections.FirstOrDefault(x => x.Id == match.AwaySelectionId)?.Name ?? "Away selection not assigned",
            selections.Select(s => new EventSelectionDto(s.PublicId, s.Name,
                members.Where(p => p.SelectionId == s.Id).Select(p => new EventParticipantDto(p.PublicId, p.Name)).ToList())).ToList());
    }
    public Task<bool> SelectionBelongsToMatchAsync(Guid matchId, Guid selectionId, CancellationToken ct) =>
        db.Matches.AnyAsync(x => x.Id == matchId && (x.HomeSelectionId == selectionId || x.AwaySelectionId == selectionId), ct);
    public Task<bool> PlayerBelongsToSelectionAsync(Guid playerId, Guid selectionId, CancellationToken ct) =>
        db.PlayerSelectionHistories.AnyAsync(x => x.PlayerId == playerId && x.SelectionId == selectionId && x.Player.DeletedAt == null, ct);
    public Task<string?> TypeCodeAsync(Guid typeId, CancellationToken ct) =>
        db.EventTypes.Where(x => x.Id == typeId).Select(x => x.Code).FirstOrDefaultAsync(ct);
    public async Task AddAsync(Event item, CancellationToken ct) => await db.Events.AddAsync(item, ct);
}
