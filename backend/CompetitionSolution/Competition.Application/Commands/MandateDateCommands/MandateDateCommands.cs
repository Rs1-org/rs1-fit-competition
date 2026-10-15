using Competition.Application.DTOs.HistoryDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using FluentValidation;
using MediatR;

namespace Competition.Application.Commands.MandateDateCommands;

public record CreateMandateDateCommand(string? StartDate, string? EndDate, Guid? PlayerSelectionHistoryId, Guid? SelectionManagerHistoryId)
    : IRequest<MandateDateDto>;

public class CreateMandateDateCommandValidator : AbstractValidator<CreateMandateDateCommand>
{
    public CreateMandateDateCommandValidator()
    {
        RuleFor(x => x.StartDate).Must(PersonValidationHelper.BeValidOptionalDate).WithMessage("Start date must be empty or yyyy-MM-dd.");
        RuleFor(x => x.EndDate).Must(PersonValidationHelper.BeValidOptionalDate).WithMessage("End date must be empty or yyyy-MM-dd.");
        RuleFor(x => x.EndDate).Must((command, end) => MandateDates.EndIsOnOrAfterStart(command.StartDate, end))
            .WithMessage("End date must be on or after the start date.");
        RuleFor(x => x.PlayerSelectionHistoryId).NotEqual(Guid.Empty).When(x => x.PlayerSelectionHistoryId.HasValue);
        RuleFor(x => x.SelectionManagerHistoryId).NotEqual(Guid.Empty).When(x => x.SelectionManagerHistoryId.HasValue);
    }
}

public class CreateMandateDateCommandHandler(IHistoryStore history, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateMandateDateCommand, MandateDateDto>
{
    public async Task<MandateDateDto> Handle(CreateMandateDateCommand request, CancellationToken cancellationToken)
    {
        await MandateDates.RequireLinks(history, request.PlayerSelectionHistoryId, request.SelectionManagerHistoryId, cancellationToken);
        var entity = new MandateDate
        {
            StartDate = NodaTimeMappingHelper.ParseNullableLocalDate(request.StartDate),
            EndDate = NodaTimeMappingHelper.ParseNullableLocalDate(request.EndDate),
            PlayerSelectionHistoryId = request.PlayerSelectionHistoryId,
            SelectionManagerHistoryId = request.SelectionManagerHistoryId
        };
        await history.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return HistoryMaps.ToDto(entity);
    }
}

public record UpdateMandateDateCommand(Guid Id, string? StartDate, string? EndDate, Guid? PlayerSelectionHistoryId, Guid? SelectionManagerHistoryId)
    : IRequest<MandateDateDto>;

public class UpdateMandateDateCommandValidator : AbstractValidator<UpdateMandateDateCommand>
{
    public UpdateMandateDateCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.StartDate).Must(PersonValidationHelper.BeValidOptionalDate).WithMessage("Start date must be empty or yyyy-MM-dd.");
        RuleFor(x => x.EndDate).Must(PersonValidationHelper.BeValidOptionalDate).WithMessage("End date must be empty or yyyy-MM-dd.");
        RuleFor(x => x.EndDate).Must((command, end) => MandateDates.EndIsOnOrAfterStart(command.StartDate, end))
            .WithMessage("End date must be on or after the start date.");
        RuleFor(x => x.PlayerSelectionHistoryId).NotEqual(Guid.Empty).When(x => x.PlayerSelectionHistoryId.HasValue);
        RuleFor(x => x.SelectionManagerHistoryId).NotEqual(Guid.Empty).When(x => x.SelectionManagerHistoryId.HasValue);
    }
}

public class UpdateMandateDateCommandHandler(IHistoryStore history, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateMandateDateCommand, MandateDateDto>
{
    public async Task<MandateDateDto> Handle(UpdateMandateDateCommand request, CancellationToken cancellationToken)
    {
        await MandateDates.RequireLinks(history, request.PlayerSelectionHistoryId, request.SelectionManagerHistoryId, cancellationToken);
        var entity = await history.GetMandateAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"MandateDate with id {request.Id} not found");

        entity.StartDate = NodaTimeMappingHelper.ParseNullableLocalDate(request.StartDate);
        entity.EndDate = NodaTimeMappingHelper.ParseNullableLocalDate(request.EndDate);
        entity.PlayerSelectionHistoryId = request.PlayerSelectionHistoryId;
        entity.SelectionManagerHistoryId = request.SelectionManagerHistoryId;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return HistoryMaps.ToDto(entity);
    }
}

public record DeleteMandateDateCommand(Guid Id) : IRequest<Guid>;

public class DeleteMandateDateCommandValidator : AbstractValidator<DeleteMandateDateCommand>
{
    public DeleteMandateDateCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class DeleteMandateDateCommandHandler(IHistoryStore history, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteMandateDateCommand, Guid>
{
    public async Task<Guid> Handle(DeleteMandateDateCommand request, CancellationToken cancellationToken)
    {
        var entity = await history.GetMandateAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"MandateDate with id {request.Id} not found");

        history.Remove(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return request.Id;
    }
}

internal static class MandateDates
{
    public static bool EndIsOnOrAfterStart(string? start, string? end) =>
        string.IsNullOrWhiteSpace(start) || string.IsNullOrWhiteSpace(end) || string.CompareOrdinal(end, start) >= 0;

    public static async Task RequireLinks(IHistoryStore history, Guid? playerSelectionHistoryId, Guid? selectionManagerHistoryId, CancellationToken cancellationToken)
    {
        if (playerSelectionHistoryId is not null && !await history.PlayerSelectionHistoryExistsAsync(playerSelectionHistoryId.Value, cancellationToken))
            throw new KeyNotFoundException($"PlayerSelectionHistory with id {playerSelectionHistoryId} not found");
        if (selectionManagerHistoryId is not null && !await history.SelectionManagerHistoryExistsAsync(selectionManagerHistoryId.Value, cancellationToken))
            throw new KeyNotFoundException($"SelectionManagerHistory with id {selectionManagerHistoryId} not found");
    }
}
