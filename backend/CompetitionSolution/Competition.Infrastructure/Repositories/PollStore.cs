using Competition.Application.Common.Models;
using Competition.Application.Polls;
using Competition.Domain.Entities.MatchEntities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Competition.Infrastructure.Repositories;

public class PollStore(AppDbContext db) : IPollStore
{
    public async Task<PagedResult<PollDto>> GetPageAsync(Guid matchId, Guid? viewerId, Instant now, int page, int pageSize, CancellationToken ct)
    {
        var result = await db.Polls.AsNoTracking().Where(x => x.MatchId == matchId)
            .OrderByDescending(x => x.CreatedAt).ThenBy(x => x.PublicId)
            .Select(x => new
            {
                x.PublicId, MatchPublicId = x.Match.PublicId, x.Question, x.OpensAt, x.ClosesAt,
                TotalVotes = x.Votes.Count(),
                MyOption = viewerId.HasValue ? x.Votes.Where(v => v.UserId == viewerId.Value).Select(v => v.Option.PublicId).FirstOrDefault() : null,
                Options = x.Options.OrderBy(o => o.SortOrder).ThenBy(o => o.PublicId)
                    .Select(o => new { o.PublicId, o.Label, Votes = o.Votes.Count() }).ToList(),
            }).ToPagedResultAsync(page, pageSize, ct);
        return new(result.Items.Select(x => new PollDto(x.PublicId, x.MatchPublicId, x.Question,
            x.OpensAt.ToDateTimeOffset(), x.ClosesAt.ToDateTimeOffset(), now.ToDateTimeOffset(), now >= x.OpensAt && now < x.ClosesAt,
            x.TotalVotes, x.MyOption, x.Options.Select(o => new PollOptionDto(o.PublicId, o.Label, o.Votes,
                x.TotalVotes == 0 ? 0 : Math.Round(100.0 * o.Votes / x.TotalVotes, 1))).ToList())).ToList(), result.TotalCount, page, pageSize);
    }
    public Task<Poll?> FindAsync(string publicId, CancellationToken ct) =>
        db.Polls.Include(x => x.Match).Include(x => x.Options).FirstOrDefaultAsync(x => x.PublicId == publicId, ct);
    public Task<bool> UserIsActiveAsync(Guid userId, CancellationToken ct) => db.Users.AnyAsync(x => x.Id == userId && x.IsActive, ct);
    public Task<bool> AlreadyVotedAsync(Guid pollId, Guid userId, CancellationToken ct) => db.PollVotes.AnyAsync(x => x.PollId == pollId && x.UserId == userId, ct);
    public async Task AddAsync(Poll poll, CancellationToken ct) => await db.Polls.AddAsync(poll, ct);
    public async Task AddVoteAsync(PollVote vote, CancellationToken ct) => await db.PollVotes.AddAsync(vote, ct);
}
