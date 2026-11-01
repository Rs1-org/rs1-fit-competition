using Competition.Application.Interfaces;
using Competition.Domain.Entities.Person;
using MediatR;

namespace Competition.Application.Commands.RefereeCommands.Delete;

public class DeleteRefereeCommandHandler(IReferee refereeRepository, IUnitOfWork unitOfWork, IPublicIdLookup lookup)
    : IRequestHandler<DeleteRefereeCommand, string>
{
    public async Task<string> Handle(DeleteRefereeCommand request, CancellationToken cancellationToken)
    {
        var id = await lookup.GetRequiredIdAsync<Referee>(request.PublicId, cancellationToken);
        var referee = await refereeRepository.GetActiveByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Referee with public id {request.PublicId} not found");

        refereeRepository.Remove(referee);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return referee.PublicId;
    }
}
