using Competition.Application.Interfaces;
using Competition.Domain.Entities.Person;
using MediatR;

namespace Competition.Application.Commands.PlayerCommands.Delete;

public class DeletePlayerCommandHandler(IPlayer playerRepository, IUnitOfWork unitOfWork, IPublicIdLookup lookup)
    : IRequestHandler<DeletePlayerCommand, string>
{
    public async Task<string> Handle(DeletePlayerCommand request, CancellationToken cancellationToken)
    {
        var id = await lookup.GetRequiredIdAsync<Player>(request.PublicId, cancellationToken);
        var player = await playerRepository.GetActiveByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Player with public id {request.PublicId} not found");

        playerRepository.Remove(player);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return player.PublicId;
    }
}
