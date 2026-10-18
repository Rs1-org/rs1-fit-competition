using Competition.Application.Common.Models;
using Competition.Domain.Entities.MatchEntities;
using NodaTime;

namespace Competition.Application.Polls;

public record PollOptionDto(string PublicId, string Label, int Votes, double Percentage);
public record PollDto(string PublicId, string MatchPublicId, string Question, DateTimeOffset OpensAt,
    DateTimeOffset ClosesAt, DateTimeOffset ServerNow, bool IsOpen, int TotalVotes, string? MyOptionPublicId, IReadOnlyList<PollOptionDto> Options);
public record SavedPollDto(string PublicId);
public interface IPollStore
{
    Task<PagedResult<PollDto>> GetPageAsync(Guid matchId, Guid? viewerId, Instant now, int page, int pageSize, CancellationToken ct);
    Task<Poll?> FindAsync(string publicId, CancellationToken ct);
    Task<bool> UserIsActiveAsync(Guid userId, CancellationToken ct);
    Task<bool> AlreadyVotedAsync(Guid pollId, Guid userId, CancellationToken ct);
    Task AddAsync(Poll poll, CancellationToken ct);
    Task AddVoteAsync(PollVote vote, CancellationToken ct);
}
