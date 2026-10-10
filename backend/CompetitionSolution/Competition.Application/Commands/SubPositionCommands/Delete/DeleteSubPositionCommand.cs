using Competition.Application.Interfaces;
using FluentValidation;
using MediatR;

namespace Competition.Application.Commands.SubPositionCommands.Delete;

public class DeleteSubPositionCommand : IRequest<Guid>
{
    public Guid Id { get; set; }
}

public class DeleteSubPositionCommandValidator : AbstractValidator<DeleteSubPositionCommand>
{
    public DeleteSubPositionCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class DeleteSubPositionCommandHandler : IRequestHandler<DeleteSubPositionCommand, Guid>
{
    private readonly ISubPosition _subPositions;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSubPositionCommandHandler(ISubPosition subPositions, IUnitOfWork unitOfWork)
    {
        _subPositions = subPositions;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(DeleteSubPositionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _subPositions.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Subposition '{request.Id}' was not found.");

        if (await _subPositions.HasPlayersAsync(entity.Id, cancellationToken))
            throw new InvalidOperationException("Subposition is assigned to players and cannot be deleted.");

        await _subPositions.DeleteAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return request.Id;
    }
}
