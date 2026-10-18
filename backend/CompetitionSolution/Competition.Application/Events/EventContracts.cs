using Competition.Application.Common.Models;
using Competition.Domain.Entities.MatchEntities;

namespace Competition.Application.Events;

public record EventLookupDto(string PublicId, string Code, string Name, int SortOrder);
public record EventLookupsDto(IReadOnlyList<EventLookupDto> EventTypes, IReadOnlyList<EventLookupDto> MatchParts);
public record EventParticipantDto(string PublicId, string Name);
public record EventSelectionDto(string PublicId, string Name, IReadOnlyList<EventParticipantDto> Players);
public record EventContextDto(string MatchPublicId, string HomeName, string AwayName, IReadOnlyList<EventSelectionDto> Selections);
public record EventDto(string PublicId, string MatchPublicId, string MatchName,
    EventLookupDto EventType, EventLookupDto MatchPart, EventParticipantDto Selection,
    EventParticipantDto? Player, EventParticipantDto? RelatedPlayer, int Minute, int AddedMinute, string? Note);
public record SavedEventDto(string PublicId);

public interface IEventStore
{
    Task<PagedResult<EventDto>> GetPageAsync(GetEventsQuery filter, CancellationToken ct);
    Task<EventDto?> GetAsync(string publicId, CancellationToken ct);
    Task<Event?> FindAsync(string publicId, CancellationToken ct);
    Task<EventLookupsDto> LookupsAsync(CancellationToken ct);
    Task<EventContextDto?> ContextAsync(string matchPublicId, CancellationToken ct);
    Task<bool> SelectionBelongsToMatchAsync(Guid matchId, Guid selectionId, CancellationToken ct);
    Task<bool> PlayerBelongsToSelectionAsync(Guid playerId, Guid selectionId, CancellationToken ct);
    Task<string?> TypeCodeAsync(Guid typeId, CancellationToken ct);
    Task AddAsync(Event item, CancellationToken ct);
}
