using Competition.Application.DTOs.PersonDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Commands.ManagerCommands.Create;

public class CreateManagerCommandHandler(IManager managerRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateManagerCommand, ManagerDto>
{
    public async Task<ManagerDto> Handle(CreateManagerCommand request, CancellationToken cancellationToken)
    {
        var entity = PersonMappingHelper.ToEntity(request.Dto);
        await managerRepository.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync();
        return PersonMappingHelper.ToDto(entity);
    }
}
