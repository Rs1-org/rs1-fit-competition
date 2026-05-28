using Competition.Application.DTOs.PersonDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Commands.RefereeCommands.Update;

public class UpdateRefereeCommandHandler(IReferee refereeRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateRefereeCommand, RefereeDto>
{
    public async Task<RefereeDto> Handle(UpdateRefereeCommand request, CancellationToken cancellationToken)
    {
        var referee = await refereeRepository.GetActiveByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Referee with id {request.Id} not found");

        PersonMappingHelper.Apply(request.Dto, referee);
        await unitOfWork.SaveChangesAsync();
        return PersonMappingHelper.ToDto(referee);
    }
}
