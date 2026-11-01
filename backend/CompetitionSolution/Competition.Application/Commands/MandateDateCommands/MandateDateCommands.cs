using Competition.Application.Common.Validation;
using Competition.Application.DTOs.HistoryDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using FluentValidation;
using MediatR;

namespace Competition.Application.Commands.MandateDateCommands;

public record CreateMandateDateCommand(string? StartDate, string? EndDate, string? PlayerSelectionHistoryPublicId, string? SelectionManagerHistoryPublicId)
    : IRequest<MandateDateDto>;

public class CreateMandateDateCommandValidator : AbstractValidator<CreateMandateDateCommand>
{
    public CreateMandateDateCommandValidator(IPublicIdLookup lookup)
    {
        RuleFor(x => x.StartDate).Must(PersonValidationHelper.BeValidOptionalDate).WithMessage("Start date must be empty or yyyy-MM-dd.");
        RuleFor(x => x.EndDate).Must(PersonValidationHelper.BeValidOptionalDate).WithMessage("End date must be empty or yyyy-MM-dd.");
        RuleFor(x => x.EndDate).Must((command, end) => MandateDates.EndIsOnOrAfterStart(command.StartDate, end))
            .WithMessage("End date must be on or after the start date.");
        RuleFor(x => x.PlayerSelectionHistoryPublicId!).MustReferenceExisting<CreateMandateDateCommand, PlayerSelectionHistory>(lookup)
            .When(x => !string.IsNullOrWhiteSpace(x.PlayerSelectionHistoryPublicId));
        RuleFor(x => x.SelectionManagerHistoryPublicId!).MustReferenceExisting<CreateMandateDateCommand, SelectionManagerHistory>(lookup)
            .When(x => !string.IsNullOrWhiteSpace(x.SelectionManagerHistoryPublicId));
    }
}

public class CreateMandateDateCommandHandler(IHistoryStore history, IUnitOfWork unitOfWork, IPublicIdLookup lookup)
    : IRequestHandler<CreateMandateDateCommand, MandateDateDto>
{
    public async Task<MandateDateDto> Handle(CreateMandateDateCommand request, CancellationToken cancellationToken)
    {
        var playerHistoryId = await lookup.GetOptionalIdAsync<PlayerSelectionHistory>(request.PlayerSelectionHistoryPublicId, cancellationToken);
        var managerHistoryId = await lookup.GetOptionalIdAsync<SelectionManagerHistory>(request.SelectionManagerHistoryPublicId, cancellationToken);
        var entity = new MandateDate
        {
            StartDate = NodaTimeMappingHelper.ParseNullableLocalDate(request.StartDate),
            EndDate = NodaTimeMappingHelper.ParseNullableLocalDate(request.EndDate),
            PlayerSelectionHistoryId = playerHistoryId,
            SelectionManagerHistoryId = managerHistoryId
        };
        await history.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return HistoryMaps.ToDto((await history.GetMandateAsync(entity.Id, cancellationToken))!);
    }
}

public record UpdateMandateDateCommand(string PublicId, string? StartDate, string? EndDate, string? PlayerSelectionHistoryPublicId, string? SelectionManagerHistoryPublicId)
    : IRequest<MandateDateDto>;

public class UpdateMandateDateCommandValidator : AbstractValidator<UpdateMandateDateCommand>
{
    public UpdateMandateDateCommandValidator(IPublicIdLookup lookup)
    {
        RuleFor(x => x.PublicId).MustBeValidPublicId();
        RuleFor(x => x.StartDate).Must(PersonValidationHelper.BeValidOptionalDate).WithMessage("Start date must be empty or yyyy-MM-dd.");
        RuleFor(x => x.EndDate).Must(PersonValidationHelper.BeValidOptionalDate).WithMessage("End date must be empty or yyyy-MM-dd.");
        RuleFor(x => x.EndDate).Must((command, end) => MandateDates.EndIsOnOrAfterStart(command.StartDate, end))
            .WithMessage("End date must be on or after the start date.");
        RuleFor(x => x.PlayerSelectionHistoryPublicId!).MustReferenceExisting<UpdateMandateDateCommand, PlayerSelectionHistory>(lookup)
            .When(x => !string.IsNullOrWhiteSpace(x.PlayerSelectionHistoryPublicId));
        RuleFor(x => x.SelectionManagerHistoryPublicId!).MustReferenceExisting<UpdateMandateDateCommand, SelectionManagerHistory>(lookup)
            .When(x => !string.IsNullOrWhiteSpace(x.SelectionManagerHistoryPublicId));
    }
}

public class UpdateMandateDateCommandHandler(IHistoryStore history, IUnitOfWork unitOfWork, IPublicIdLookup lookup)
    : IRequestHandler<UpdateMandateDateCommand, MandateDateDto>
{
    public async Task<MandateDateDto> Handle(UpdateMandateDateCommand request, CancellationToken cancellationToken)
    {
        var id = await lookup.GetRequiredIdAsync<MandateDate>(request.PublicId, cancellationToken);
        var playerHistoryId = await lookup.GetOptionalIdAsync<PlayerSelectionHistory>(request.PlayerSelectionHistoryPublicId, cancellationToken);
        var managerHistoryId = await lookup.GetOptionalIdAsync<SelectionManagerHistory>(request.SelectionManagerHistoryPublicId, cancellationToken);
        var entity = await history.GetMandateAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"MandateDate with public id {request.PublicId} not found");

        entity.StartDate = NodaTimeMappingHelper.ParseNullableLocalDate(request.StartDate);
        entity.EndDate = NodaTimeMappingHelper.ParseNullableLocalDate(request.EndDate);
        entity.PlayerSelectionHistoryId = playerHistoryId;
        entity.SelectionManagerHistoryId = managerHistoryId;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return HistoryMaps.ToDto((await history.GetMandateAsync(entity.Id, cancellationToken))!);
    }
}

public record DeleteMandateDateCommand(string PublicId) : IRequest<string>;

public class DeleteMandateDateCommandValidator : AbstractValidator<DeleteMandateDateCommand>
{
    public DeleteMandateDateCommandValidator() => RuleFor(x => x.PublicId).MustBeValidPublicId();
}

public class DeleteMandateDateCommandHandler(IHistoryStore history, IUnitOfWork unitOfWork, IPublicIdLookup lookup)
    : IRequestHandler<DeleteMandateDateCommand, string>
{
    public async Task<string> Handle(DeleteMandateDateCommand request, CancellationToken cancellationToken)
    {
        var id = await lookup.GetRequiredIdAsync<MandateDate>(request.PublicId, cancellationToken);
        var entity = await history.GetMandateAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"MandateDate with public id {request.PublicId} not found");
        history.Remove(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return entity.PublicId;
    }
}

internal static class MandateDates
{
    public static bool EndIsOnOrAfterStart(string? start, string? end) =>
        string.IsNullOrWhiteSpace(start) || string.IsNullOrWhiteSpace(end) || string.CompareOrdinal(end, start) >= 0;
}
