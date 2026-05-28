using AutoMapper;
using Competition.Application.DTOs.PersonDto;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.Person;
using MediatR;

namespace Competition.Application.Features.RefereeCommands.Create;

public class CreateRefereeCommandHandler(IReferee refereeRepository, IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<CreateRefereeCommand, RefereeDto>
{
    public async Task<RefereeDto> Handle(CreateRefereeCommand request, CancellationToken cancellationToken)
    {
        var entity = mapper.Map<Referee>(request);
        await refereeRepository.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync();
        return mapper.Map<RefereeDto>(entity);
    }
}
