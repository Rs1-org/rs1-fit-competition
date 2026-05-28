using Competition.Application.DTOs.PersonDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Commands.RefereeCommands.Create;

public class CreateRefereeCommandHandler(IReferee refereeRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateRefereeCommand, RefereeDto>
{
    public async Task<RefereeDto> Handle(CreateRefereeCommand request, CancellationToken cancellationToken)
    {
        var entity = PersonMappingHelper.ToEntity(request.Dto);
        await refereeRepository.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync();
        return PersonMappingHelper.ToDto(entity);
    }
}
