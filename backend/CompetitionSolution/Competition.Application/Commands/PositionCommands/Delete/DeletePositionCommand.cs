using Competition.Application.Common.Validation;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.FormationEntities;
using FluentValidation;
using MediatR;

namespace Competition.Application.Commands.PositionCommands.Delete;

public class DeletePositionCommand : IRequest<string>
{
    public string PublicId { get; set; } = string.Empty;
}

public class DeletePositionCommandValidator : AbstractValidator<DeletePositionCommand>
{
    public DeletePositionCommandValidator() => RuleFor(x => x.PublicId).MustBeValidPublicId();
}

public class DeletePositionCommandHandler : IRequestHandler<DeletePositionCommand, string>
{
    private readonly IPosition _positions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublicIdLookup _lookup;

    public DeletePositionCommandHandler(IPosition positions, IUnitOfWork unitOfWork, IPublicIdLookup lookup)
    {
        _positions = positions;
        _unitOfWork = unitOfWork;
        _lookup = lookup;
    }

    public async Task<string> Handle(DeletePositionCommand request, CancellationToken cancellationToken)
    {
        var id = await _lookup.GetRequiredIdAsync<Position>(request.PublicId, cancellationToken);
        var entity = await _positions.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Position '{request.PublicId}' was not found.");

        if (await _positions.HasSubPositionsAsync(entity.Id, cancellationToken))
            throw new InvalidOperationException("Position still has subpositions and cannot be deleted.");
        if (await _positions.HasPlayersAsync(entity.Id, cancellationToken))
            throw new InvalidOperationException("Position is assigned to players and cannot be deleted.");

        await _positions.DeleteAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return entity.PublicId;
    }
}
