using Competition.Application.DTOs.HistoryDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using FluentValidation;
using MediatR;

namespace Competition.Application.Commands.TransferValueHistoryCommands;

public record CreateTransferValueHistoryCommand(string? Date, decimal? Value, Guid PlayerId) : IRequest<TransferValueHistoryDto>;

public class CreateTransferValueHistoryCommandValidator : AbstractValidator<CreateTransferValueHistoryCommand>
{
    public CreateTransferValueHistoryCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.Date).Must(PersonValidationHelper.BeValidOptionalDate).WithMessage("Date must be empty or yyyy-MM-dd.");
        RuleFor(x => x.Value).GreaterThanOrEqualTo(0).PrecisionScale(18, 2, true).When(x => x.Value.HasValue);
    }
}

public class CreateTransferValueHistoryCommandHandler(IHistoryStore history, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateTransferValueHistoryCommand, TransferValueHistoryDto>
{
    public async Task<TransferValueHistoryDto> Handle(CreateTransferValueHistoryCommand request, CancellationToken cancellationToken)
    {
        if (!await history.PlayerExistsAsync(request.PlayerId, cancellationToken))
            throw new KeyNotFoundException($"Player with id {request.PlayerId} not found");

        var entity = new TransferValueHistory
        {
            Date = NodaTimeMappingHelper.ParseNullableLocalDate(request.Date),
            Value = request.Value,
            PlayerId = request.PlayerId
        };
        await history.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return HistoryMaps.ToDto(entity);
    }
}

public record UpdateTransferValueHistoryCommand(Guid Id, string? Date, decimal? Value, Guid PlayerId) : IRequest<TransferValueHistoryDto>;

public class UpdateTransferValueHistoryCommandValidator : AbstractValidator<UpdateTransferValueHistoryCommand>
{
    public UpdateTransferValueHistoryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.Date).Must(PersonValidationHelper.BeValidOptionalDate).WithMessage("Date must be empty or yyyy-MM-dd.");
        RuleFor(x => x.Value).GreaterThanOrEqualTo(0).PrecisionScale(18, 2, true).When(x => x.Value.HasValue);
    }
}

public class UpdateTransferValueHistoryCommandHandler(IHistoryStore history, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateTransferValueHistoryCommand, TransferValueHistoryDto>
{
    public async Task<TransferValueHistoryDto> Handle(UpdateTransferValueHistoryCommand request, CancellationToken cancellationToken)
    {
        if (!await history.PlayerExistsAsync(request.PlayerId, cancellationToken))
            throw new KeyNotFoundException($"Player with id {request.PlayerId} not found");

        var entity = await history.GetTransferAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"TransferValueHistory with id {request.Id} not found");

        entity.Date = NodaTimeMappingHelper.ParseNullableLocalDate(request.Date);
        entity.Value = request.Value;
        entity.PlayerId = request.PlayerId;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return HistoryMaps.ToDto(entity);
    }
}

public record DeleteTransferValueHistoryCommand(Guid Id) : IRequest<Guid>;

public class DeleteTransferValueHistoryCommandValidator : AbstractValidator<DeleteTransferValueHistoryCommand>
{
    public DeleteTransferValueHistoryCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class DeleteTransferValueHistoryCommandHandler(IHistoryStore history, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteTransferValueHistoryCommand, Guid>
{
    public async Task<Guid> Handle(DeleteTransferValueHistoryCommand request, CancellationToken cancellationToken)
    {
        var entity = await history.GetTransferAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"TransferValueHistory with id {request.Id} not found");

        history.Remove(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return request.Id;
    }
}
