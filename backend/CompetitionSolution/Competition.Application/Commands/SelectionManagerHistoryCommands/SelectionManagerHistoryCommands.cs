using Competition.Application.DTOs.HistoryDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using FluentValidation;
using MediatR;

namespace Competition.Application.Commands.SelectionManagerHistoryCommands;

public record CreateSelectionManagerHistoryCommand(string? Notes, int? Wins, int? Loses, int? Draw, Guid ManagerId, Guid? SelectionId)
    : IRequest<SelectionManagerHistoryDto>;

public class CreateSelectionManagerHistoryCommandValidator : AbstractValidator<CreateSelectionManagerHistoryCommand>
{
    public CreateSelectionManagerHistoryCommandValidator()
    {
        RuleFor(x => x.ManagerId).NotEmpty();
        RuleFor(x => x.SelectionId).NotEqual(Guid.Empty).When(x => x.SelectionId.HasValue);
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.Wins).GreaterThanOrEqualTo(0).When(x => x.Wins.HasValue);
        RuleFor(x => x.Loses).GreaterThanOrEqualTo(0).When(x => x.Loses.HasValue);
        RuleFor(x => x.Draw).GreaterThanOrEqualTo(0).When(x => x.Draw.HasValue);
    }
}

public class CreateSelectionManagerHistoryCommandHandler(IHistoryStore history, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateSelectionManagerHistoryCommand, SelectionManagerHistoryDto>
{
    public async Task<SelectionManagerHistoryDto> Handle(CreateSelectionManagerHistoryCommand request, CancellationToken cancellationToken)
    {
        await RequireManagerAndSelection(request.ManagerId, request.SelectionId, cancellationToken);
        var entity = new SelectionManagerHistory
        {
            Notes = request.Notes,
            Wins = request.Wins,
            Loses = request.Loses,
            Draw = request.Draw,
            ManagerId = request.ManagerId,
            SelectionId = request.SelectionId
        };
        await history.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return HistoryMaps.ToDto(entity);
    }

    private async Task RequireManagerAndSelection(Guid managerId, Guid? selectionId, CancellationToken cancellationToken)
    {
        if (!await history.ManagerExistsAsync(managerId, cancellationToken))
            throw new KeyNotFoundException($"Manager with id {managerId} not found");
        if (selectionId is not null && !await history.SelectionExistsAsync(selectionId.Value, cancellationToken))
            throw new KeyNotFoundException($"Selection with id {selectionId} not found");
    }
}

public record UpdateSelectionManagerHistoryCommand(Guid Id, string? Notes, int? Wins, int? Loses, int? Draw, Guid ManagerId, Guid? SelectionId)
    : IRequest<SelectionManagerHistoryDto>;

public class UpdateSelectionManagerHistoryCommandValidator : AbstractValidator<UpdateSelectionManagerHistoryCommand>
{
    public UpdateSelectionManagerHistoryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ManagerId).NotEmpty();
        RuleFor(x => x.SelectionId).NotEqual(Guid.Empty).When(x => x.SelectionId.HasValue);
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.Wins).GreaterThanOrEqualTo(0).When(x => x.Wins.HasValue);
        RuleFor(x => x.Loses).GreaterThanOrEqualTo(0).When(x => x.Loses.HasValue);
        RuleFor(x => x.Draw).GreaterThanOrEqualTo(0).When(x => x.Draw.HasValue);
    }
}

public class UpdateSelectionManagerHistoryCommandHandler(IHistoryStore history, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSelectionManagerHistoryCommand, SelectionManagerHistoryDto>
{
    public async Task<SelectionManagerHistoryDto> Handle(UpdateSelectionManagerHistoryCommand request, CancellationToken cancellationToken)
    {
        if (!await history.ManagerExistsAsync(request.ManagerId, cancellationToken))
            throw new KeyNotFoundException($"Manager with id {request.ManagerId} not found");
        if (request.SelectionId is not null && !await history.SelectionExistsAsync(request.SelectionId.Value, cancellationToken))
            throw new KeyNotFoundException($"Selection with id {request.SelectionId} not found");

        var entity = await history.GetManagerSelectionAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"SelectionManagerHistory with id {request.Id} not found");

        entity.Notes = request.Notes;
        entity.Wins = request.Wins;
        entity.Loses = request.Loses;
        entity.Draw = request.Draw;
        entity.ManagerId = request.ManagerId;
        entity.SelectionId = request.SelectionId;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return HistoryMaps.ToDto(entity);
    }
}

public record DeleteSelectionManagerHistoryCommand(Guid Id) : IRequest<Guid>;

public class DeleteSelectionManagerHistoryCommandValidator : AbstractValidator<DeleteSelectionManagerHistoryCommand>
{
    public DeleteSelectionManagerHistoryCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class DeleteSelectionManagerHistoryCommandHandler(IHistoryStore history, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteSelectionManagerHistoryCommand, Guid>
{
    public async Task<Guid> Handle(DeleteSelectionManagerHistoryCommand request, CancellationToken cancellationToken)
    {
        var entity = await history.GetManagerSelectionAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"SelectionManagerHistory with id {request.Id} not found");

        history.Remove(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return request.Id;
    }
}
