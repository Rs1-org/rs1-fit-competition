using Competition.Application.Interfaces;
using Competition.Domain.Entities.Person;
using MediatR;

namespace Competition.Application.Commands.ManagerCommands.Delete;

public class DeleteManagerCommandHandler(IManager managerRepository, IUnitOfWork unitOfWork, IPublicIdLookup lookup)
    : IRequestHandler<DeleteManagerCommand, string>
{
    public async Task<string> Handle(DeleteManagerCommand request, CancellationToken cancellationToken)
    {
        var id = await lookup.GetRequiredIdAsync<Manager>(request.PublicId, cancellationToken);
        var manager = await managerRepository.GetActiveByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Manager with public id {request.PublicId} not found");

        managerRepository.Remove(manager);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return manager.PublicId;
    }
}
