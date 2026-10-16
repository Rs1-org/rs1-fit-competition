using Competition.Application.Commands.MatchCommands.Create;
using Competition.Application.Commands.MatchCommands.Delete;
using Competition.Application.Commands.MatchCommands.Update;
using Competition.Application.Common.Validation;
using FluentValidation;

namespace Competition.Application.Commands.MatchCommands.Validators;

public class CreateMatchCommandValidator : AbstractValidator<CreateMatchCommand>
{
    public CreateMatchCommandValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Date)
            .NotEmpty()
            .Must(d => d > DateTime.UtcNow)
            .WithMessage("Match date must be in the future for a new match.");

        RuleFor(x => x.StatusPublicId)
            .MustBeValidPublicId();

        RuleFor(x => x.StadiumPublicId)
            .MustBeValidPublicId();
        RuleFor(x => x.HomeSelectionPublicId)
            .MustBeValidPublicId()
            .When(x => x.HomeSelectionPublicId is not null);
        RuleFor(x => x.AwaySelectionPublicId)
            .MustBeValidPublicId()
            .When(x => x.AwaySelectionPublicId is not null);
        RuleFor(x => x.RoundPublicId)
            .MustBeValidPublicId()
            .When(x => x.RoundPublicId is not null);

        RuleFor(x => x.FollowersCount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x)
            .Must(x => x.HomeSelectionPublicId != x.AwaySelectionPublicId || x.HomeSelectionPublicId is null)
            .WithMessage("Home and away selection must be different.");
    }
}

public class UpdateMatchCommandValidator : AbstractValidator<UpdateMatchCommand>
{
    public UpdateMatchCommandValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty();

        RuleFor(x => x.StatusPublicId)
            .MustBeValidPublicId();

        RuleFor(x => x.StadiumPublicId)
            .MustBeValidPublicId();
        RuleFor(x => x.HomeSelectionPublicId)
            .MustBeValidPublicId()
            .When(x => x.HomeSelectionPublicId is not null);
        RuleFor(x => x.AwaySelectionPublicId)
            .MustBeValidPublicId()
            .When(x => x.AwaySelectionPublicId is not null);
        RuleFor(x => x.RoundPublicId)
            .MustBeValidPublicId()
            .When(x => x.RoundPublicId is not null);

        RuleFor(x => x.FollowersCount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.HomeScore)
            .GreaterThanOrEqualTo(0)
            .When(x => x.HomeScore.HasValue);

        RuleFor(x => x.AwayScore)
            .GreaterThanOrEqualTo(0)
            .When(x => x.AwayScore.HasValue);

        RuleFor(x => x)
            .Must(x => x.HomeScore.HasValue == x.AwayScore.HasValue)
            .WithMessage("HomeScore and AwayScore must both be set or both be null.");

        RuleFor(x => x.HalfHomeScore)
            .GreaterThanOrEqualTo(0)
            .When(x => x.HalfHomeScore.HasValue)
            .LessThanOrEqualTo(x => x.HomeScore ?? int.MaxValue)
            .When(x => x.HalfHomeScore.HasValue && x.HomeScore.HasValue)
            .WithMessage("Half-time home score cannot exceed full-time home score.");

        RuleFor(x => x.HalfAwayScore)
            .GreaterThanOrEqualTo(0)
            .When(x => x.HalfAwayScore.HasValue)
            .LessThanOrEqualTo(x => x.AwayScore ?? int.MaxValue)
            .When(x => x.HalfAwayScore.HasValue && x.AwayScore.HasValue)
            .WithMessage("Half-time away score cannot exceed full-time away score.");

        RuleFor(x => x)
            .Must(x => x.HalfHomeScore.HasValue == x.HalfAwayScore.HasValue)
            .WithMessage("HalfHomeScore and HalfAwayScore must both be set or both be null.");

        RuleFor(x => x)
            .Must(x => x.HomeSelectionPublicId != x.AwaySelectionPublicId || x.HomeSelectionPublicId is null)
            .WithMessage("Home and away selection must be different.");
    }
}

public class DeleteMatchCommandValidator : AbstractValidator<DeleteMatchCommand>
{
    public DeleteMatchCommandValidator()
    {
        RuleFor(x => x.PublicId)
            .MustBeValidPublicId();
    }
}
