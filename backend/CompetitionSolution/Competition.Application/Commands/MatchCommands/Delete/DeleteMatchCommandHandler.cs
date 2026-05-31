using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Commands.MatchCommands.Delete;

public class DeleteMatchCommandHandler : IRequestHandler<DeleteMatchCommand, Guid>
{
    private readonly IMatch _match;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteMatchCommandHandler(IMatch match, IUnitOfWork unitOfWork)
    {
        _match = match;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(DeleteMatchCommand request, CancellationToken cancellationToken)
    {
        var match = await _match.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Match with ID '{request.Id}' not found.");

        await _match.DeleteAsync(match, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return request.Id;
    }
}
