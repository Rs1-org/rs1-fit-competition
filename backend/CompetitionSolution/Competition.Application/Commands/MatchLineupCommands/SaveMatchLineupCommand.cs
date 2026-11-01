using Competition.Application.Common.Validation;
using Competition.Application.DTOs.MatchDto;
using Competition.Application.Interfaces;
using Competition.Application.Lineups;
using Competition.Domain.Entities;
using Competition.Domain.Entities.FormationEntities;
using Competition.Domain.Entities.MatchEntities;
using Competition.Domain.Entities.Person;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using System.Text.Json.Serialization;

namespace Competition.Application.Commands.MatchLineupCommands;

public class SaveMatchLineupCommand : IRequest<MatchLineupDto>
{
    [JsonIgnore]
    public string PublicId { get; set; } = string.Empty;
    public string? HomeFormationPublicId { get; set; }
    public string? AwayFormationPublicId { get; set; }
    public List<SaveLineupPlayerDto> Players { get; set; } = [];
}

public class SaveMatchLineupCommandValidator : AbstractValidator<SaveMatchLineupCommand>
{
    public SaveMatchLineupCommandValidator(IPublicIdLookup lookup)
    {
        RuleFor(x => x.PublicId).MustBeValidPublicId();
        RuleFor(x => x.HomeFormationPublicId!).MustReferenceExisting<SaveMatchLineupCommand, Formation>(lookup)
            .When(x => !string.IsNullOrWhiteSpace(x.HomeFormationPublicId));
        RuleFor(x => x.AwayFormationPublicId!).MustReferenceExisting<SaveMatchLineupCommand, Formation>(lookup)
            .When(x => !string.IsNullOrWhiteSpace(x.AwayFormationPublicId));
        RuleFor(x => x.Players).NotNull();
        When(x => x.Players != null, () =>
        {
            RuleForEach(x => x.Players).ChildRules(player =>
            {
                player.RuleFor(x => x.PlayerPublicId).MustReferenceExisting<SaveLineupPlayerDto, Player>(lookup);
                player.RuleFor(x => x.SelectionPublicId).MustReferenceExisting<SaveLineupPlayerDto, Selection>(lookup);
                player.RuleFor(x => x.PositionPublicId!).MustReferenceExisting<SaveLineupPlayerDto, Position>(lookup)
                    .When(x => !string.IsNullOrWhiteSpace(x.PositionPublicId));
                player.RuleFor(x => x.SubPositionPublicId!).MustReferenceExisting<SaveLineupPlayerDto, SubPosition>(lookup)
                    .When(x => !string.IsNullOrWhiteSpace(x.SubPositionPublicId));
                player.RuleFor(x => x.ShirtNumber).InclusiveBetween(1, 99).When(x => x.ShirtNumber.HasValue);
            });
            RuleFor(x => x.Players).Must(players => players!.Select(p => p.PlayerPublicId).Distinct().Count() == players!.Count)
                .WithMessage("A player can appear only once in the match.");
            RuleFor(x => x.Players).Must(players => players!
                    .Where(p => p.ShirtNumber.HasValue)
                    .GroupBy(p => (p.SelectionPublicId, p.ShirtNumber))
                    .All(group => group.Count() == 1))
                .WithMessage("Shirt numbers must be unique for each team.");
            RuleFor(x => x.Players).Must(players => players!
                    .Where(p => p.IsStarter)
                    .GroupBy(p => p.SelectionPublicId)
                    .All(group => group.Count() <= 11))
                .WithMessage("A team can have at most 11 starters.");
        });
    }
}

public class SaveMatchLineupCommandHandler(IMatchLineup lineups, IUnitOfWork unitOfWork, IPublicIdLookup lookup, IMediator mediator)
    : IRequestHandler<SaveMatchLineupCommand, MatchLineupDto>
{
    public async Task<MatchLineupDto> Handle(SaveMatchLineupCommand request, CancellationToken cancellationToken)
    {
        var matchId = await lookup.GetRequiredIdAsync<Match>(request.PublicId, cancellationToken);
        var match = await lineups.GetMatchAsync(matchId, cancellationToken)
            ?? throw new KeyNotFoundException($"Match with public id '{request.PublicId}' not found.");

        var homeId = match.HomeSelectionId;
        var awayId = match.AwaySelectionId;
        match.HomeFormationId = await lookup.GetOptionalIdAsync<Formation>(request.HomeFormationPublicId, cancellationToken);
        match.AwayFormationId = await lookup.GetOptionalIdAsync<Formation>(request.AwayFormationPublicId, cancellationToken);

        foreach (var existing in match.Lineups.ToList())
            lineups.Remove(existing);

        foreach (var player in request.Players)
        {
            var selectionId = await lookup.GetRequiredIdAsync<Selection>(player.SelectionPublicId, cancellationToken);
            if (selectionId != homeId && selectionId != awayId)
                throw Failure("Selection is not playing in this match.");

            var playerId = await lookup.GetRequiredIdAsync<Player>(player.PlayerPublicId, cancellationToken);
            if (!await lineups.PlayerInSelectionAsync(playerId, selectionId, cancellationToken))
                throw Failure("Only players of that selection can be selected.");

            var positionId = await lookup.GetOptionalIdAsync<Position>(player.PositionPublicId, cancellationToken);
            var subPositionId = await lookup.GetOptionalIdAsync<SubPosition>(player.SubPositionPublicId, cancellationToken);
            if (subPositionId.HasValue)
            {
                var subId = await lookup.GetRequiredIdAsync<SubPosition>(player.SubPositionPublicId, cancellationToken);
                var sub = await lookup.GetRequiredEntityAsync<SubPosition>(player.SubPositionPublicId, cancellationToken);
                if (sub.PositionId != positionId)
                    throw Failure("Subposition does not belong to the selected position.");
                subPositionId = subId;
            }

            await lineups.AddAsync(new MatchLineup
            {
                MatchId = match.Id,
                SelectionId = selectionId,
                PlayerId = playerId,
                PositionId = positionId,
                SubPositionId = subPositionId,
                IsStarter = player.IsStarter,
                ShirtNumber = player.ShirtNumber
            }, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return await mediator.Send(new GetMatchLineupQuery(request.PublicId), cancellationToken);
    }

    private static ValidationException Failure(string message) =>
        new([new ValidationFailure("Players", message)]);
}
