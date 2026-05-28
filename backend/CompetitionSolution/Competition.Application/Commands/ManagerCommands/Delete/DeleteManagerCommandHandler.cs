using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Features.ManagerCommands.Delete;

public class DeleteManagerCommandHandler(IManager managerRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteManagerCommand, Guid>
{
    public async Task<Guid> Handle(DeleteManagerCommand request, CancellationToken cancellationToken)
    {
        var manager = await managerRepository.GetActiveByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Manager with id {request.Id} not found");

        managerRepository.Remove(manager);
        await unitOfWork.SaveChangesAsync();
        return request.Id;
    }
}
