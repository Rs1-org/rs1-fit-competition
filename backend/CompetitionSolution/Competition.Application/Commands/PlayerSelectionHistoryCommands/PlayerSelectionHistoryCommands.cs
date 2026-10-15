using Competition.Application.DTOs.HistoryDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using FluentValidation;
using MediatR;

namespace Competition.Application.Commands.PlayerSelectionHistoryCommands;

public record CreatePlayerSelectionHistoryCommand(int? Goals, int? MatchPlayed, int? Assists, int? ShirtNumber, Guid PlayerId, Guid? SelectionId)
    : IRequest<PlayerSelectionHistoryDto>;

public class CreatePlayerSelectionHistoryCommandValidator : AbstractValidator<CreatePlayerSelectionHistoryCommand>
{
    public CreatePlayerSelectionHistoryCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.SelectionId).NotEqual(Guid.Empty).When(x => x.SelectionId.HasValue);
        RuleFor(x => x.Goals).GreaterThanOrEqualTo(0).When(x => x.Goals.HasValue);
        RuleFor(x => x.MatchPlayed).GreaterThanOrEqualTo(0).When(x => x.MatchPlayed.HasValue);
        RuleFor(x => x.Assists).GreaterThanOrEqualTo(0).When(x => x.Assists.HasValue);
        RuleFor(x => x.ShirtNumber).InclusiveBetween(1, 99).When(x => x.ShirtNumber.HasValue);
    }
}

public class CreatePlayerSelectionHistoryCommandHandler(IHistoryStore history, IUnitOfWork unitOfWork)
    : IRequestHandler<CreatePlayerSelectionHistoryCommand, PlayerSelectionHistoryDto>
{
    public async Task<PlayerSelectionHistoryDto> Handle(CreatePlayerSelectionHistoryCommand request, CancellationToken cancellationToken)
    {
        await HistoryGuards.RequirePlayerAndSelection(history, request.PlayerId, request.SelectionId, cancellationToken);
        var entity = new PlayerSelectionHistory
        {
            Goals = request.Goals,
            MatchPlayed = request.MatchPlayed,
            Assists = request.Assists,
            ShirtNumber = request.ShirtNumber,
            PlayerId = request.PlayerId,
            SelectionId = request.SelectionId
        };
        await history.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return HistoryMaps.ToDto(entity);
    }
}

public record UpdatePlayerSelectionHistoryCommand(Guid Id, int? Goals, int? MatchPlayed, int? Assists, int? ShirtNumber, Guid PlayerId, Guid? SelectionId)
    : IRequest<PlayerSelectionHistoryDto>;

public class UpdatePlayerSelectionHistoryCommandValidator : AbstractValidator<UpdatePlayerSelectionHistoryCommand>
{
    public UpdatePlayerSelectionHistoryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.SelectionId).NotEqual(Guid.Empty).When(x => x.SelectionId.HasValue);
        RuleFor(x => x.Goals).GreaterThanOrEqualTo(0).When(x => x.Goals.HasValue);
        RuleFor(x => x.MatchPlayed).GreaterThanOrEqualTo(0).When(x => x.MatchPlayed.HasValue);
        RuleFor(x => x.Assists).GreaterThanOrEqualTo(0).When(x => x.Assists.HasValue);
        RuleFor(x => x.ShirtNumber).InclusiveBetween(1, 99).When(x => x.ShirtNumber.HasValue);
    }
}

public class UpdatePlayerSelectionHistoryCommandHandler(IHistoryStore history, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdatePlayerSelectionHistoryCommand, PlayerSelectionHistoryDto>
{
    public async Task<PlayerSelectionHistoryDto> Handle(UpdatePlayerSelectionHistoryCommand request, CancellationToken cancellationToken)
    {
        await HistoryGuards.RequirePlayerAndSelection(history, request.PlayerId, request.SelectionId, cancellationToken);
        var entity = await history.GetPlayerSelectionAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"PlayerSelectionHistory with id {request.Id} not found");

        entity.Goals = request.Goals;
        entity.MatchPlayed = request.MatchPlayed;
        entity.Assists = request.Assists;
        entity.ShirtNumber = request.ShirtNumber;
        entity.PlayerId = request.PlayerId;
        entity.SelectionId = request.SelectionId;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return HistoryMaps.ToDto(entity);
    }
}

public record DeletePlayerSelectionHistoryCommand(Guid Id) : IRequest<Guid>;

public class DeletePlayerSelectionHistoryCommandValidator : AbstractValidator<DeletePlayerSelectionHistoryCommand>
{
    public DeletePlayerSelectionHistoryCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class DeletePlayerSelectionHistoryCommandHandler(IHistoryStore history, IUnitOfWork unitOfWork)
    : IRequestHandler<DeletePlayerSelectionHistoryCommand, Guid>
{
    public async Task<Guid> Handle(DeletePlayerSelectionHistoryCommand request, CancellationToken cancellationToken)
    {
        var entity = await history.GetPlayerSelectionAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"PlayerSelectionHistory with id {request.Id} not found");

        history.Remove(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return request.Id;
    }
}

internal static class HistoryGuards
{
    public static async Task RequirePlayerAndSelection(IHistoryStore history, Guid playerId, Guid? selectionId, CancellationToken cancellationToken)
    {
        if (!await history.PlayerExistsAsync(playerId, cancellationToken))
            throw new KeyNotFoundException($"Player with id {playerId} not found");
        if (selectionId is not null && !await history.SelectionExistsAsync(selectionId.Value, cancellationToken))
            throw new KeyNotFoundException($"Selection with id {selectionId} not found");
    }
}
