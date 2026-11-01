using Competition.Application.Common.Validation;
using Competition.Application.DTOs.HistoryDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using Competition.Domain.Entities.Person;
using FluentValidation;
using MediatR;

namespace Competition.Application.Commands.SelectionManagerHistoryCommands;

public record CreateSelectionManagerHistoryCommand(string? Notes, int? Wins, int? Loses, int? Draw, string ManagerPublicId, string? SelectionPublicId)
    : IRequest<SelectionManagerHistoryDto>;

public class CreateSelectionManagerHistoryCommandValidator : AbstractValidator<CreateSelectionManagerHistoryCommand>
{
    public CreateSelectionManagerHistoryCommandValidator(IPublicIdLookup lookup)
    {
        RuleFor(x => x.ManagerPublicId).MustReferenceExisting<CreateSelectionManagerHistoryCommand, Manager>(lookup);
        RuleFor(x => x.SelectionPublicId!).MustReferenceExisting<CreateSelectionManagerHistoryCommand, Selection>(lookup)
            .When(x => !string.IsNullOrWhiteSpace(x.SelectionPublicId));
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.Wins).GreaterThanOrEqualTo(0).When(x => x.Wins.HasValue);
        RuleFor(x => x.Loses).GreaterThanOrEqualTo(0).When(x => x.Loses.HasValue);
        RuleFor(x => x.Draw).GreaterThanOrEqualTo(0).When(x => x.Draw.HasValue);
    }
}

public class CreateSelectionManagerHistoryCommandHandler(IHistoryStore history, IUnitOfWork unitOfWork, IPublicIdLookup lookup)
    : IRequestHandler<CreateSelectionManagerHistoryCommand, SelectionManagerHistoryDto>
{
    public async Task<SelectionManagerHistoryDto> Handle(CreateSelectionManagerHistoryCommand request, CancellationToken cancellationToken)
    {
        var entity = new SelectionManagerHistory
        {
            Notes = request.Notes,
            Wins = request.Wins,
            Loses = request.Loses,
            Draw = request.Draw,
            ManagerId = await lookup.GetRequiredIdAsync<Manager>(request.ManagerPublicId, cancellationToken),
            SelectionId = await lookup.GetOptionalIdAsync<Selection>(request.SelectionPublicId, cancellationToken)
        };
        await history.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return HistoryMaps.ToDto((await history.GetManagerSelectionAsync(entity.Id, cancellationToken))!);
    }
}

public record UpdateSelectionManagerHistoryCommand(string PublicId, string? Notes, int? Wins, int? Loses, int? Draw, string ManagerPublicId, string? SelectionPublicId)
    : IRequest<SelectionManagerHistoryDto>;

public class UpdateSelectionManagerHistoryCommandValidator : AbstractValidator<UpdateSelectionManagerHistoryCommand>
{
    public UpdateSelectionManagerHistoryCommandValidator(IPublicIdLookup lookup)
    {
        RuleFor(x => x.PublicId).MustBeValidPublicId();
        RuleFor(x => x.ManagerPublicId).MustReferenceExisting<UpdateSelectionManagerHistoryCommand, Manager>(lookup);
        RuleFor(x => x.SelectionPublicId!).MustReferenceExisting<UpdateSelectionManagerHistoryCommand, Selection>(lookup)
            .When(x => !string.IsNullOrWhiteSpace(x.SelectionPublicId));
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.Wins).GreaterThanOrEqualTo(0).When(x => x.Wins.HasValue);
        RuleFor(x => x.Loses).GreaterThanOrEqualTo(0).When(x => x.Loses.HasValue);
        RuleFor(x => x.Draw).GreaterThanOrEqualTo(0).When(x => x.Draw.HasValue);
    }
}

public class UpdateSelectionManagerHistoryCommandHandler(IHistoryStore history, IUnitOfWork unitOfWork, IPublicIdLookup lookup)
    : IRequestHandler<UpdateSelectionManagerHistoryCommand, SelectionManagerHistoryDto>
{
    public async Task<SelectionManagerHistoryDto> Handle(UpdateSelectionManagerHistoryCommand request, CancellationToken cancellationToken)
    {
        var id = await lookup.GetRequiredIdAsync<SelectionManagerHistory>(request.PublicId, cancellationToken);
        var entity = await history.GetManagerSelectionAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"SelectionManagerHistory with public id {request.PublicId} not found");
        entity.Notes = request.Notes;
        entity.Wins = request.Wins;
        entity.Loses = request.Loses;
        entity.Draw = request.Draw;
        entity.ManagerId = await lookup.GetRequiredIdAsync<Manager>(request.ManagerPublicId, cancellationToken);
        entity.SelectionId = await lookup.GetOptionalIdAsync<Selection>(request.SelectionPublicId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return HistoryMaps.ToDto((await history.GetManagerSelectionAsync(entity.Id, cancellationToken))!);
    }
}

public record DeleteSelectionManagerHistoryCommand(string PublicId) : IRequest<string>;

public class DeleteSelectionManagerHistoryCommandValidator : AbstractValidator<DeleteSelectionManagerHistoryCommand>
{
    public DeleteSelectionManagerHistoryCommandValidator() => RuleFor(x => x.PublicId).MustBeValidPublicId();
}

public class DeleteSelectionManagerHistoryCommandHandler(IHistoryStore history, IUnitOfWork unitOfWork, IPublicIdLookup lookup)
    : IRequestHandler<DeleteSelectionManagerHistoryCommand, string>
{
    public async Task<string> Handle(DeleteSelectionManagerHistoryCommand request, CancellationToken cancellationToken)
    {
        var id = await lookup.GetRequiredIdAsync<SelectionManagerHistory>(request.PublicId, cancellationToken);
        var entity = await history.GetManagerSelectionAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"SelectionManagerHistory with public id {request.PublicId} not found");
        history.Remove(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return entity.PublicId;
    }
}
