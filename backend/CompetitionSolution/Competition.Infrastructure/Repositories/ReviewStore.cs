using Competition.Application.Common.Models;
using Competition.Application.Reviews;
using Competition.Domain.Entities;
using Competition.Domain.Entities.MatchEntities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using NodaTime.Text;

namespace Competition.Infrastructure.Repositories;

public class ReviewStore(AppDbContext db) : IReviewStore
{
    public async Task<MatchReviewPage> GetPageAsync(Guid matchId, Guid? viewerId, int page, int pageSize, CancellationToken ct)
    {
        var query = db.MatchReviews.AsNoTracking().Where(x => x.MatchId == matchId);
        var average = await query.AverageAsync(x => (double?)x.Rating, ct);
        var result = await query.Include(x => x.Author).OrderByDescending(x => x.CreatedAt).ThenBy(x => x.PublicId)
            .ToPagedResultAsync(page, pageSize, ct);
        var own = viewerId.HasValue ? await query.Include(x => x.Author).FirstOrDefaultAsync(x => x.AuthorId == viewerId.Value, ct) : null;
        var ids = result.Items.Select(x => x.Id).ToList();
        if (own is not null && !ids.Contains(own.Id)) ids.Add(own.Id);
        var counts = await db.ReviewReactions.AsNoTracking().Where(x => ids.Contains(x.ReviewId)).GroupBy(x => x.ReviewId)
            .Select(g => new { ReviewId = g.Key, Likes = g.Count(x => x.Value == 1), Dislikes = g.Count(x => x.Value == -1) }).ToListAsync(ct);
        var viewerReactions = viewerId.HasValue ? await db.ReviewReactions.AsNoTracking()
            .Where(x => ids.Contains(x.ReviewId) && x.UserId == viewerId.Value).ToListAsync(ct) : [];
        ReviewDto Map(MatchReview review)
        {
            var count = counts.FirstOrDefault(x => x.ReviewId == review.Id);
            return new ReviewDto(review.PublicId, review.Rating, review.Comment, DisplayName(review.Author),
                InstantPattern.ExtendedIso.Format(review.CreatedAt), review.AuthorId == viewerId,
                count?.Likes ?? 0, count?.Dislikes ?? 0, viewerReactions.FirstOrDefault(x => x.ReviewId == review.Id)?.Value);
        }
        return new MatchReviewPage(average.HasValue ? Math.Round(average.Value, 2) : null, result.TotalCount,
            new PagedResult<ReviewDto>(result.Items.Select(Map).ToList(), result.TotalCount, page, pageSize), own is null ? null : Map(own));
    }

    private static string DisplayName(ApplicationUser author) => string.IsNullOrWhiteSpace(author.FirstName)
        ? "Competition user" : author.FirstName.Trim() + (string.IsNullOrWhiteSpace(author.LastName) ? "" : $" {System.Globalization.StringInfo.GetNextTextElement(author.LastName.Trim())}.");
    public Task<bool> UserIsActiveAsync(Guid userId, CancellationToken ct) => db.Users.AnyAsync(x => x.Id == userId && x.IsActive, ct);
    public Task<bool> AlreadyReviewedAsync(Guid matchId, Guid authorId, CancellationToken ct) =>
        db.MatchReviews.AnyAsync(x => x.MatchId == matchId && x.AuthorId == authorId, ct);
    public Task<MatchReview?> FindAsync(string publicId, CancellationToken ct) => db.MatchReviews.FirstOrDefaultAsync(x => x.PublicId == publicId, ct);
    public async Task AddAsync(MatchReview review, CancellationToken ct) => await db.MatchReviews.AddAsync(review, ct);
    public Task<ReviewReaction?> FindReactionAsync(Guid reviewId, Guid userId, CancellationToken ct) =>
        db.ReviewReactions.FirstOrDefaultAsync(x => x.ReviewId == reviewId && x.UserId == userId, ct);
    public async Task AddReactionAsync(ReviewReaction reaction, CancellationToken ct) => await db.ReviewReactions.AddAsync(reaction, ct);
}
