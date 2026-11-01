using Competition.Application.Common.Validation;
using Competition.Application.DTOs.MatchDto;
using Competition.Application.Interfaces;
using Competition.Application.Lineups;
using Competition.Domain.Entities.MatchEntities;
using Competition.Domain.Entities.Person;
using FluentValidation;
using MediatR;
using System.Text.Json.Serialization;

namespace Competition.Application.Commands.MatchLineupCommands;

public class AssignMatchRefereeCommand : IRequest<MatchLineupDto>
{
    [JsonIgnore]
    public string PublicId { get; set; } = string.Empty;
    public string? RefereePublicId { get; set; }
}

public class AssignMatchRefereeCommandValidator : AbstractValidator<AssignMatchRefereeCommand>
{
    public AssignMatchRefereeCommandValidator(IPublicIdLookup lookup)
    {
        RuleFor(x => x.PublicId).MustBeValidPublicId();
        RuleFor(x => x.RefereePublicId!).MustReferenceExisting<AssignMatchRefereeCommand, Referee>(lookup)
            .When(x => !string.IsNullOrWhiteSpace(x.RefereePublicId));
    }
}

public class AssignMatchRefereeCommandHandler(IMatchLineup lineups, IUnitOfWork unitOfWork, IPublicIdLookup lookup, IMediator mediator)
    : IRequestHandler<AssignMatchRefereeCommand, MatchLineupDto>
{
    public async Task<MatchLineupDto> Handle(AssignMatchRefereeCommand request, CancellationToken cancellationToken)
    {
        var matchId = await lookup.GetRequiredIdAsync<Match>(request.PublicId, cancellationToken);
        var match = await lineups.GetMatchAsync(matchId, cancellationToken)
            ?? throw new KeyNotFoundException($"Match with public id '{request.PublicId}' not found.");

        match.RefereeId = await lookup.GetOptionalIdAsync<Referee>(request.RefereePublicId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return await mediator.Send(new GetMatchLineupQuery(request.PublicId), cancellationToken);
    }
}
