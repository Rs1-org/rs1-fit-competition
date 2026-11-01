using Competition.Application.Common.Validation;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.FormationEntities;
using FluentValidation;
using MediatR;

namespace Competition.Application.Commands.SubPositionCommands.Delete;

public class DeleteSubPositionCommand : IRequest<string>
{
    public string PublicId { get; set; } = string.Empty;
}

public class DeleteSubPositionCommandValidator : AbstractValidator<DeleteSubPositionCommand>
{
    public DeleteSubPositionCommandValidator() => RuleFor(x => x.PublicId).MustBeValidPublicId();
}

public class DeleteSubPositionCommandHandler : IRequestHandler<DeleteSubPositionCommand, string>
{
    private readonly ISubPosition _subPositions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublicIdLookup _lookup;

    public DeleteSubPositionCommandHandler(ISubPosition subPositions, IUnitOfWork unitOfWork, IPublicIdLookup lookup)
    {
        _subPositions = subPositions;
        _unitOfWork = unitOfWork;
        _lookup = lookup;
    }

    public async Task<string> Handle(DeleteSubPositionCommand request, CancellationToken cancellationToken)
    {
        var id = await _lookup.GetRequiredIdAsync<SubPosition>(request.PublicId, cancellationToken);
        var entity = await _subPositions.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Subposition '{request.PublicId}' was not found.");

        if (await _subPositions.HasPlayersAsync(entity.Id, cancellationToken))
            throw new InvalidOperationException("Subposition is assigned to players and cannot be deleted.");

        await _subPositions.DeleteAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return entity.PublicId;
    }
}
