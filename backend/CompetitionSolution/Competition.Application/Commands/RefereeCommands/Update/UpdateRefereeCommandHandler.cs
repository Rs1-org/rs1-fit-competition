using AutoMapper;
using Competition.Application.DTOs.PersonDto;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.Person;
using MediatR;

namespace Competition.Application.Commands.RefereeCommands.Update;

public class UpdateRefereeCommandHandler(IReferee refereeRepository, IUnitOfWork unitOfWork, IMapper mapper, IPublicIdLookup lookup)
    : IRequestHandler<UpdateRefereeCommand, RefereeDto>
{
    public async Task<RefereeDto> Handle(UpdateRefereeCommand request, CancellationToken cancellationToken)
    {
        var id = await lookup.GetRequiredIdAsync<Referee>(request.PublicId, cancellationToken);
        var referee = await refereeRepository.GetActiveByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Referee with public id {request.PublicId} not found");

        mapper.Map(request, referee);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return mapper.Map<RefereeDto>(referee);
    }
}
