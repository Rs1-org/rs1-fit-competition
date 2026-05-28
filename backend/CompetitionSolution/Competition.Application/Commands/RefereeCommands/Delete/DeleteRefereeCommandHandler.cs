using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Features.RefereeCommands.Delete;

public class DeleteRefereeCommandHandler(IReferee refereeRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteRefereeCommand, Guid>
{
    public async Task<Guid> Handle(DeleteRefereeCommand request, CancellationToken cancellationToken)
    {
        var referee = await refereeRepository.GetActiveByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Referee with id {request.Id} not found");

        refereeRepository.Remove(referee);
        await unitOfWork.SaveChangesAsync();
        return request.Id;
    }
}
