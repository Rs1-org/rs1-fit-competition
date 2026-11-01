using Competition.Application.Common.Validation;
using Competition.Application.DTOs.HistoryDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using Competition.Domain.Entities.Person;
using FluentValidation;
using MediatR;

namespace Competition.Application.Commands.TransferValueHistoryCommands;

public record CreateTransferValueHistoryCommand(string? Date, decimal? Value, string PlayerPublicId) : IRequest<TransferValueHistoryDto>;

public class CreateTransferValueHistoryCommandValidator : AbstractValidator<CreateTransferValueHistoryCommand>
{
    public CreateTransferValueHistoryCommandValidator(IPublicIdLookup lookup)
    {
        RuleFor(x => x.PlayerPublicId).MustReferenceExisting<CreateTransferValueHistoryCommand, Player>(lookup);
        RuleFor(x => x.Date).Must(PersonValidationHelper.BeValidOptionalDate).WithMessage("Date must be empty or yyyy-MM-dd.");
        RuleFor(x => x.Value).GreaterThanOrEqualTo(0).PrecisionScale(18, 2, true).When(x => x.Value.HasValue);
    }
}

public class CreateTransferValueHistoryCommandHandler(IHistoryStore history, IUnitOfWork unitOfWork, IPublicIdLookup lookup)
    : IRequestHandler<CreateTransferValueHistoryCommand, TransferValueHistoryDto>
{
    public async Task<TransferValueHistoryDto> Handle(CreateTransferValueHistoryCommand request, CancellationToken cancellationToken)
    {
        var playerId = await lookup.GetRequiredIdAsync<Player>(request.PlayerPublicId, cancellationToken);
        var entity = new TransferValueHistory
        {
            Date = NodaTimeMappingHelper.ParseNullableLocalDate(request.Date),
            Value = request.Value,
            PlayerId = playerId
        };
        await history.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return HistoryMaps.ToDto((await history.GetTransferAsync(entity.Id, cancellationToken))!);
    }
}

public record UpdateTransferValueHistoryCommand(string PublicId, string? Date, decimal? Value, string PlayerPublicId) : IRequest<TransferValueHistoryDto>;

public class UpdateTransferValueHistoryCommandValidator : AbstractValidator<UpdateTransferValueHistoryCommand>
{
    public UpdateTransferValueHistoryCommandValidator(IPublicIdLookup lookup)
    {
        RuleFor(x => x.PublicId).MustBeValidPublicId();
        RuleFor(x => x.PlayerPublicId).MustReferenceExisting<UpdateTransferValueHistoryCommand, Player>(lookup);
        RuleFor(x => x.Date).Must(PersonValidationHelper.BeValidOptionalDate).WithMessage("Date must be empty or yyyy-MM-dd.");
        RuleFor(x => x.Value).GreaterThanOrEqualTo(0).PrecisionScale(18, 2, true).When(x => x.Value.HasValue);
    }
}

public class UpdateTransferValueHistoryCommandHandler(IHistoryStore history, IUnitOfWork unitOfWork, IPublicIdLookup lookup)
    : IRequestHandler<UpdateTransferValueHistoryCommand, TransferValueHistoryDto>
{
    public async Task<TransferValueHistoryDto> Handle(UpdateTransferValueHistoryCommand request, CancellationToken cancellationToken)
    {
        var id = await lookup.GetRequiredIdAsync<TransferValueHistory>(request.PublicId, cancellationToken);
        var playerId = await lookup.GetRequiredIdAsync<Player>(request.PlayerPublicId, cancellationToken);
        var entity = await history.GetTransferAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"TransferValueHistory with public id {request.PublicId} not found");

        entity.Date = NodaTimeMappingHelper.ParseNullableLocalDate(request.Date);
        entity.Value = request.Value;
        entity.PlayerId = playerId;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return HistoryMaps.ToDto((await history.GetTransferAsync(entity.Id, cancellationToken))!);
    }
}

public record DeleteTransferValueHistoryCommand(string PublicId) : IRequest<string>;

public class DeleteTransferValueHistoryCommandValidator : AbstractValidator<DeleteTransferValueHistoryCommand>
{
    public DeleteTransferValueHistoryCommandValidator() => RuleFor(x => x.PublicId).MustBeValidPublicId();
}

public class DeleteTransferValueHistoryCommandHandler(IHistoryStore history, IUnitOfWork unitOfWork, IPublicIdLookup lookup)
    : IRequestHandler<DeleteTransferValueHistoryCommand, string>
{
    public async Task<string> Handle(DeleteTransferValueHistoryCommand request, CancellationToken cancellationToken)
    {
        var id = await lookup.GetRequiredIdAsync<TransferValueHistory>(request.PublicId, cancellationToken);
        var entity = await history.GetTransferAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"TransferValueHistory with public id {request.PublicId} not found");
        history.Remove(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return entity.PublicId;
    }
}
