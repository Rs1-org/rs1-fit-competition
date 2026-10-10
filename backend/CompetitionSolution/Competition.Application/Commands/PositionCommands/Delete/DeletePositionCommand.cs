using Competition.Application.Interfaces;
using FluentValidation;
using MediatR;

namespace Competition.Application.Commands.PositionCommands.Delete;

public class DeletePositionCommand : IRequest<Guid>
{
    public Guid Id { get; set; }
}

public class DeletePositionCommandValidator : AbstractValidator<DeletePositionCommand>
{
    public DeletePositionCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class DeletePositionCommandHandler : IRequestHandler<DeletePositionCommand, Guid>
{
    private readonly IPosition _positions;
    private readonly IUnitOfWork _unitOfWork;

    public DeletePositionCommandHandler(IPosition positions, IUnitOfWork unitOfWork)
    {
        _positions = positions;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(DeletePositionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _positions.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Position '{request.Id}' was not found.");

        if (await _positions.HasSubPositionsAsync(entity.Id, cancellationToken))
            throw new InvalidOperationException("Position still has subpositions and cannot be deleted.");
        if (await _positions.HasPlayersAsync(entity.Id, cancellationToken))
            throw new InvalidOperationException("Position is assigned to players and cannot be deleted.");

        await _positions.DeleteAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return request.Id;
    }
}
