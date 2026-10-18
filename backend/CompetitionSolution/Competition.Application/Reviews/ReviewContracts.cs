using Competition.Application.Common.Models;
using Competition.Domain.Entities.MatchEntities;

namespace Competition.Application.Reviews;

public record ReviewDto(string PublicId, int Rating, string Comment, string AuthorName, string CreatedAt,
    bool IsMine, int LikeCount, int DislikeCount, int? MyReaction);
public record MatchReviewPage(double? AverageRating, int ReviewCount, PagedResult<ReviewDto> Reviews, ReviewDto? MyReview);
public record SavedReviewDto(string PublicId);
public record ReviewReactionResult(int? Value);

public interface IReviewStore
{
    Task<MatchReviewPage> GetPageAsync(Guid matchId, Guid? viewerId, int page, int pageSize, CancellationToken ct);
    Task<bool> UserIsActiveAsync(Guid userId, CancellationToken ct);
    Task<bool> AlreadyReviewedAsync(Guid matchId, Guid authorId, CancellationToken ct);
    Task<MatchReview?> FindAsync(string publicId, CancellationToken ct);
    Task AddAsync(MatchReview review, CancellationToken ct);
    Task<ReviewReaction?> FindReactionAsync(Guid reviewId, Guid userId, CancellationToken ct);
    Task AddReactionAsync(ReviewReaction reaction, CancellationToken ct);
}
