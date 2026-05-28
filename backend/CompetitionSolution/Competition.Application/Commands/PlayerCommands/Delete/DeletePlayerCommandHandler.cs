using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Features.PlayerCommands.Delete;

public class DeletePlayerCommandHandler(IPlayer playerRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeletePlayerCommand, Guid>
{
    public async Task<Guid> Handle(DeletePlayerCommand request, CancellationToken cancellationToken)
    {
        var player = await playerRepository.GetActiveByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Player with id {request.Id} not found");

        playerRepository.Remove(player);
        await unitOfWork.SaveChangesAsync();
        return request.Id;
    }
}
