using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Commands.RoundCommands.Delete;

public class DeleteRoundCommandHandler : IRequestHandler<DeleteRoundCommand, Guid>
{
    private readonly IRound _round;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteRoundCommandHandler(IRound round, IUnitOfWork unitOfWork)
    {
        _round = round;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(DeleteRoundCommand request, CancellationToken cancellationToken)
    {
        var round = await _round.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Round with ID '{request.Id}' not found.");

        // Block instead of cascading so no match loses its round.
        if (await _round.HasMatchesAsync(round.Id, cancellationToken))
            throw BusinessRule.Violation(nameof(request.Id), "This round still has matches. Move or delete its matches first.");

        await _round.DeleteAsync(round, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return request.Id;
    }
}
