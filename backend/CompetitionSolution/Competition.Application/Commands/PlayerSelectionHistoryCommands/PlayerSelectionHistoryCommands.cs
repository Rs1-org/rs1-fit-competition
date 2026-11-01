using Competition.Application.Common.Validation;
using Competition.Application.DTOs.HistoryDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using Competition.Domain.Entities.Person;
using FluentValidation;
using MediatR;

namespace Competition.Application.Commands.PlayerSelectionHistoryCommands;

public record CreatePlayerSelectionHistoryCommand(int? Goals, int? MatchPlayed, int? Assists, int? ShirtNumber, string PlayerPublicId, string? SelectionPublicId)
    : IRequest<PlayerSelectionHistoryDto>;

public class CreatePlayerSelectionHistoryCommandValidator : AbstractValidator<CreatePlayerSelectionHistoryCommand>
{
    public CreatePlayerSelectionHistoryCommandValidator(IPublicIdLookup lookup)
    {
        RuleFor(x => x.PlayerPublicId).MustReferenceExisting<CreatePlayerSelectionHistoryCommand, Player>(lookup);
        RuleFor(x => x.SelectionPublicId!).MustReferenceExisting<CreatePlayerSelectionHistoryCommand, Selection>(lookup)
            .When(x => !string.IsNullOrWhiteSpace(x.SelectionPublicId));
        RuleFor(x => x.Goals).GreaterThanOrEqualTo(0).When(x => x.Goals.HasValue);
        RuleFor(x => x.MatchPlayed).GreaterThanOrEqualTo(0).When(x => x.MatchPlayed.HasValue);
        RuleFor(x => x.Assists).GreaterThanOrEqualTo(0).When(x => x.Assists.HasValue);
        RuleFor(x => x.ShirtNumber).InclusiveBetween(1, 99).When(x => x.ShirtNumber.HasValue);
    }
}

public class CreatePlayerSelectionHistoryCommandHandler(IHistoryStore history, IUnitOfWork unitOfWork, IPublicIdLookup lookup)
    : IRequestHandler<CreatePlayerSelectionHistoryCommand, PlayerSelectionHistoryDto>
{
    public async Task<PlayerSelectionHistoryDto> Handle(CreatePlayerSelectionHistoryCommand request, CancellationToken cancellationToken)
    {
        var playerId = await lookup.GetRequiredIdAsync<Player>(request.PlayerPublicId, cancellationToken);
        var selectionId = await lookup.GetOptionalIdAsync<Selection>(request.SelectionPublicId, cancellationToken);
        var entity = new PlayerSelectionHistory
        {
            Goals = request.Goals,
            MatchPlayed = request.MatchPlayed,
            Assists = request.Assists,
            ShirtNumber = request.ShirtNumber,
            PlayerId = playerId,
            SelectionId = selectionId
        };
        await history.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return HistoryMaps.ToDto((await history.GetPlayerSelectionAsync(entity.Id, cancellationToken))!);
    }
}

public record UpdatePlayerSelectionHistoryCommand(string PublicId, int? Goals, int? MatchPlayed, int? Assists, int? ShirtNumber, string PlayerPublicId, string? SelectionPublicId)
    : IRequest<PlayerSelectionHistoryDto>;

public class UpdatePlayerSelectionHistoryCommandValidator : AbstractValidator<UpdatePlayerSelectionHistoryCommand>
{
    public UpdatePlayerSelectionHistoryCommandValidator(IPublicIdLookup lookup)
    {
        RuleFor(x => x.PublicId).MustBeValidPublicId();
        RuleFor(x => x.PlayerPublicId).MustReferenceExisting<UpdatePlayerSelectionHistoryCommand, Player>(lookup);
        RuleFor(x => x.SelectionPublicId!).MustReferenceExisting<UpdatePlayerSelectionHistoryCommand, Selection>(lookup)
            .When(x => !string.IsNullOrWhiteSpace(x.SelectionPublicId));
        RuleFor(x => x.Goals).GreaterThanOrEqualTo(0).When(x => x.Goals.HasValue);
        RuleFor(x => x.MatchPlayed).GreaterThanOrEqualTo(0).When(x => x.MatchPlayed.HasValue);
        RuleFor(x => x.Assists).GreaterThanOrEqualTo(0).When(x => x.Assists.HasValue);
        RuleFor(x => x.ShirtNumber).InclusiveBetween(1, 99).When(x => x.ShirtNumber.HasValue);
    }
}

public class UpdatePlayerSelectionHistoryCommandHandler(IHistoryStore history, IUnitOfWork unitOfWork, IPublicIdLookup lookup)
    : IRequestHandler<UpdatePlayerSelectionHistoryCommand, PlayerSelectionHistoryDto>
{
    public async Task<PlayerSelectionHistoryDto> Handle(UpdatePlayerSelectionHistoryCommand request, CancellationToken cancellationToken)
    {
        var id = await lookup.GetRequiredIdAsync<PlayerSelectionHistory>(request.PublicId, cancellationToken);
        var playerId = await lookup.GetRequiredIdAsync<Player>(request.PlayerPublicId, cancellationToken);
        var selectionId = await lookup.GetOptionalIdAsync<Selection>(request.SelectionPublicId, cancellationToken);
        var entity = await history.GetPlayerSelectionAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"PlayerSelectionHistory with public id {request.PublicId} not found");

        entity.Goals = request.Goals;
        entity.MatchPlayed = request.MatchPlayed;
        entity.Assists = request.Assists;
        entity.ShirtNumber = request.ShirtNumber;
        entity.PlayerId = playerId;
        entity.SelectionId = selectionId;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return HistoryMaps.ToDto((await history.GetPlayerSelectionAsync(entity.Id, cancellationToken))!);
    }
}

public record DeletePlayerSelectionHistoryCommand(string PublicId) : IRequest<string>;

public class DeletePlayerSelectionHistoryCommandValidator : AbstractValidator<DeletePlayerSelectionHistoryCommand>
{
    public DeletePlayerSelectionHistoryCommandValidator() => RuleFor(x => x.PublicId).MustBeValidPublicId();
}

public class DeletePlayerSelectionHistoryCommandHandler(IHistoryStore history, IUnitOfWork unitOfWork, IPublicIdLookup lookup)
    : IRequestHandler<DeletePlayerSelectionHistoryCommand, string>
{
    public async Task<string> Handle(DeletePlayerSelectionHistoryCommand request, CancellationToken cancellationToken)
    {
        var id = await lookup.GetRequiredIdAsync<PlayerSelectionHistory>(request.PublicId, cancellationToken);
        var entity = await history.GetPlayerSelectionAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"PlayerSelectionHistory with public id {request.PublicId} not found");

        history.Remove(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return entity.PublicId;
    }
}
