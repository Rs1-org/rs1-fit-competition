using System.Text.Json.Serialization;
using Competition.Application.Auth.Interfaces;
using Competition.Application.Common.Validation;
using Competition.Application.Exeptions;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.MatchEntities;
using FluentValidation;
using MediatR;
using NodaTime;

namespace Competition.Application.Reviews;

public abstract class SaveReviewCommand
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}
public class CreateReviewCommand : SaveReviewCommand, IRequest<SavedReviewDto>
{
    [JsonIgnore] public string MatchPublicId { get; set; } = string.Empty;
}
public class UpdateReviewCommand : SaveReviewCommand, IRequest<SavedReviewDto>
{
    [JsonIgnore] public string PublicId { get; set; } = string.Empty;
}
public record DeleteReviewCommand(string PublicId) : IRequest<SavedReviewDto>;
public class SetReviewReactionCommand : IRequest<ReviewReactionResult>
{
    [JsonIgnore] public string PublicId { get; set; } = string.Empty;
    public int Value { get; set; }
}
public record GetMatchReviewsQuery(string MatchPublicId, int Page, int PageSize, Guid? ViewerId) : IRequest<MatchReviewPage>;

public class SaveReviewCommandValidator<T> : AbstractValidator<T> where T : SaveReviewCommand
{
    public SaveReviewCommandValidator()
    {
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.Comment).NotEmpty().MaximumLength(2000);
    }
}
public class CreateReviewCommandValidator : SaveReviewCommandValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator() => RuleFor(x => x.MatchPublicId).MustBeValidPublicId();
}
public class UpdateReviewCommandValidator : SaveReviewCommandValidator<UpdateReviewCommand>
{
    public UpdateReviewCommandValidator() => RuleFor(x => x.PublicId).MustBeValidPublicId();
}
public class DeleteReviewCommandValidator : AbstractValidator<DeleteReviewCommand>
{
    public DeleteReviewCommandValidator() => RuleFor(x => x.PublicId).MustBeValidPublicId();
}
public class SetReviewReactionCommandValidator : AbstractValidator<SetReviewReactionCommand>
{
    public SetReviewReactionCommandValidator()
    {
        RuleFor(x => x.PublicId).MustBeValidPublicId();
        RuleFor(x => x.Value).Must(x => x is -1 or 1).WithMessage("Choose like (1) or dislike (-1).");
    }
}
public class GetMatchReviewsQueryValidator : AbstractValidator<GetMatchReviewsQuery>
{
    public GetMatchReviewsQueryValidator()
    {
        RuleFor(x => x.MatchPublicId).MustBeValidPublicId();
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

internal static class ReviewRules
{
    public static async Task ActiveUserAsync(IReviewStore store, Guid userId, CancellationToken ct)
    {
        if (!await store.UserIsActiveAsync(userId, ct))
            throw new UnauthorizedAccessException("An active account is required.");
    }
    public static async Task<MatchReview> OwnedAsync(IReviewStore store, string publicId, Guid userId, CancellationToken ct)
    {
        await ActiveUserAsync(store, userId, ct);
        var review = await store.FindAsync(publicId, ct) ?? throw new KeyNotFoundException("Review not found.");
        if (review.AuthorId != userId) throw new ForbiddenAccessException("You can only change your own review.");
        return review;
    }
}

public class CreateReviewCommandHandler(IReviewStore store, IPublicIdLookup lookup, ICurrentUser current, IUnitOfWork unit)
    : IRequestHandler<CreateReviewCommand, SavedReviewDto>
{
    public async Task<SavedReviewDto> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        var userId = current.UserId;
        await ReviewRules.ActiveUserAsync(store, userId, cancellationToken);
        var matchId = await lookup.GetRequiredIdAsync<Match>(request.MatchPublicId, cancellationToken);
        if (await store.AlreadyReviewedAsync(matchId, userId, cancellationToken))
            throw BusinessRule.Violation("MatchPublicId", "You have already reviewed this match. Edit your existing review.");
        var review = new MatchReview { MatchId = matchId, AuthorId = userId, Rating = request.Rating, Comment = request.Comment.Trim() };
        await store.AddAsync(review, cancellationToken);
        await unit.SaveChangesAsync(cancellationToken);
        return new SavedReviewDto(review.PublicId);
    }
}
public class UpdateReviewCommandHandler(IReviewStore store, ICurrentUser current, IUnitOfWork unit)
    : IRequestHandler<UpdateReviewCommand, SavedReviewDto>
{
    public async Task<SavedReviewDto> Handle(UpdateReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await ReviewRules.OwnedAsync(store, request.PublicId, current.UserId, cancellationToken);
        review.Rating = request.Rating;
        review.Comment = request.Comment.Trim();
        await unit.SaveChangesAsync(cancellationToken);
        return new SavedReviewDto(review.PublicId);
    }
}
public class DeleteReviewCommandHandler(IReviewStore store, ICurrentUser current, IUnitOfWork unit, IClock clock)
    : IRequestHandler<DeleteReviewCommand, SavedReviewDto>
{
    public async Task<SavedReviewDto> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await ReviewRules.OwnedAsync(store, request.PublicId, current.UserId, cancellationToken);
        review.DeletedAt = clock.GetCurrentInstant();
        await unit.SaveChangesAsync(cancellationToken);
        return new SavedReviewDto(review.PublicId);
    }
}
public class SetReviewReactionCommandHandler(IReviewStore store, ICurrentUser current, IUnitOfWork unit, IClock clock)
    : IRequestHandler<SetReviewReactionCommand, ReviewReactionResult>
{
    public async Task<ReviewReactionResult> Handle(SetReviewReactionCommand request, CancellationToken cancellationToken)
    {
        var userId = current.UserId;
        await ReviewRules.ActiveUserAsync(store, userId, cancellationToken);
        var review = await store.FindAsync(request.PublicId, cancellationToken) ?? throw new KeyNotFoundException("Review not found.");
        if (review.AuthorId == userId) throw new ForbiddenAccessException("React to another user's review.");
        var reaction = await store.FindReactionAsync(review.Id, userId, cancellationToken);
        if (reaction is null)
        {
            reaction = new ReviewReaction { ReviewId = review.Id, UserId = userId, Value = request.Value };
            await store.AddReactionAsync(reaction, cancellationToken);
        }
        else if (reaction.Value == request.Value) reaction.DeletedAt = clock.GetCurrentInstant();
        else reaction.Value = request.Value;
        await unit.SaveChangesAsync(cancellationToken);
        return new ReviewReactionResult(reaction.DeletedAt is null ? reaction.Value : null);
    }
}
public class GetMatchReviewsQueryHandler(IReviewStore store, IPublicIdLookup lookup)
    : IRequestHandler<GetMatchReviewsQuery, MatchReviewPage>
{
    public async Task<MatchReviewPage> Handle(GetMatchReviewsQuery request, CancellationToken cancellationToken) =>
        await store.GetPageAsync(await lookup.GetRequiredIdAsync<Match>(request.MatchPublicId, cancellationToken),
            request.ViewerId, request.Page, request.PageSize, cancellationToken);
}
