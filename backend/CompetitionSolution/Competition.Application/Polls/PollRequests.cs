using System.Text.Json.Serialization;
using Competition.Application.Auth.Interfaces;
using Competition.Application.Common.Models;
using Competition.Application.Common.Validation;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Application.Live;
using Competition.Domain.Entities.MatchEntities;
using FluentValidation;
using MediatR;
using NodaTime;

namespace Competition.Application.Polls;

public record GetMatchPollsQuery(string MatchPublicId, int Page, int PageSize, Guid? ViewerId) : IRequest<PagedResult<PollDto>>;
public class CreatePollCommand : IRequest<SavedPollDto>
{
    [JsonIgnore] public string MatchPublicId { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public List<string> Options { get; set; } = [];
    public DateTimeOffset OpensAt { get; set; }
    public DateTimeOffset ClosesAt { get; set; }
}
public class VotePollCommand : IRequest<SavedPollDto>
{
    [JsonIgnore] public string PollPublicId { get; set; } = string.Empty;
    public string OptionPublicId { get; set; } = string.Empty;
}
public class GetMatchPollsValidator : AbstractValidator<GetMatchPollsQuery>
{
    public GetMatchPollsValidator()
    {
        RuleFor(x => x.MatchPublicId).MustBeValidPublicId();
        RuleFor(x => x.Page).InclusiveBetween(1, 1000000);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}
public class CreatePollValidator : AbstractValidator<CreatePollCommand>
{
    public CreatePollValidator(IClock clock)
    {
        RuleFor(x => x.MatchPublicId).MustBeValidPublicId();
        RuleFor(x => x.Question).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Options).NotNull().Must(options => options is { Count: >= 2 and <= 8 }).WithMessage("Provide 2 to 8 options.");
        RuleForEach(x => x.Options).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Options).Must(options => options is null || options.Select(x => x?.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() == options.Count)
            .WithMessage("Options must be different, ignoring case and surrounding spaces.");
        RuleFor(x => x.OpensAt).NotEmpty();
        RuleFor(x => x.ClosesAt).NotEmpty().GreaterThan(x => x.OpensAt)
            .Must(value => Instant.FromDateTimeOffset(value) > clock.GetCurrentInstant()).WithMessage("Closing time must be in the future.");
    }
}
public class VotePollValidator : AbstractValidator<VotePollCommand>
{
    public VotePollValidator()
    {
        RuleFor(x => x.PollPublicId).MustBeValidPublicId();
        RuleFor(x => x.OptionPublicId).MustBeValidPublicId();
    }
}
public class GetMatchPollsHandler(IPollStore store, IPublicIdLookup lookup, IClock clock) : IRequestHandler<GetMatchPollsQuery, PagedResult<PollDto>>
{
    public async Task<PagedResult<PollDto>> Handle(GetMatchPollsQuery request, CancellationToken cancellationToken) =>
        await store.GetPageAsync(await lookup.GetRequiredIdAsync<Match>(request.MatchPublicId, cancellationToken),
            request.ViewerId, clock.GetCurrentInstant(), request.Page, request.PageSize, cancellationToken);
}
public class CreatePollHandler(IPollStore store, IPublicIdLookup lookup, IUnitOfWork unit, IPublisher publisher, IClock clock)
    : IRequestHandler<CreatePollCommand, SavedPollDto>
{
    public async Task<SavedPollDto> Handle(CreatePollCommand request, CancellationToken cancellationToken)
    {
        var matchId = await lookup.GetRequiredIdAsync<Match>(request.MatchPublicId, cancellationToken);
        var opens = Instant.FromDateTimeOffset(request.OpensAt); var closes = Instant.FromDateTimeOffset(request.ClosesAt);
        if (closes <= clock.GetCurrentInstant() || closes <= opens)
            throw BusinessRule.Violation(nameof(request.ClosesAt), "Choose a future closing time after opening time.");
        var poll = new Poll { MatchId = matchId, Question = request.Question.Trim(), OpensAt = opens, ClosesAt = closes,
            Options = request.Options.Select((label, index) => new PollOption { Label = label.Trim(), SortOrder = index }).ToList() };
        await store.AddAsync(poll, cancellationToken); await unit.SaveChangesAsync(cancellationToken);
        await publisher.Publish(new MatchChangedNotification(request.MatchPublicId, "polls"), cancellationToken);
        return new(poll.PublicId);
    }
}
public class VotePollHandler(IPollStore store, ICurrentUser current, IUnitOfWork unit, IPublisher publisher, IClock clock)
    : IRequestHandler<VotePollCommand, SavedPollDto>
{
    public async Task<SavedPollDto> Handle(VotePollCommand request, CancellationToken cancellationToken)
    {
        var userId = current.UserId;
        if (!await store.UserIsActiveAsync(userId, cancellationToken)) throw new UnauthorizedAccessException("An active account is required.");
        var poll = await store.FindAsync(request.PollPublicId, cancellationToken) ?? throw new KeyNotFoundException("Poll not found.");
        var now = clock.GetCurrentInstant();
        if (now < poll.OpensAt || now >= poll.ClosesAt)
            throw BusinessRule.Violation("PollPublicId", "Voting is only allowed while this poll is open.");
        if (await store.AlreadyVotedAsync(poll.Id, userId, cancellationToken))
            throw BusinessRule.Violation("PollPublicId", "You have already voted in this poll. Votes cannot be changed.");
        var option = poll.Options.SingleOrDefault(x => x.PublicId == request.OptionPublicId)
            ?? throw BusinessRule.Violation(nameof(request.OptionPublicId), "Choose an option from this poll.");
        await store.AddVoteAsync(new PollVote { PollId = poll.Id, OptionId = option.Id, UserId = userId }, cancellationToken);
        await unit.SaveChangesAsync(cancellationToken);
        await publisher.Publish(new MatchChangedNotification(poll.Match.PublicId, "polls"), cancellationToken);
        return new(poll.PublicId);
    }
}
