using AutoMapper;
using Competition.Application.DTOs.PersonDto;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.Person;
using MediatR;

namespace Competition.Application.Features.ManagerCommands.Create;

public class CreateManagerCommandHandler(IManager managerRepository, IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<CreateManagerCommand, ManagerDto>
{
    public async Task<ManagerDto> Handle(CreateManagerCommand request, CancellationToken cancellationToken)
    {
        var entity = mapper.Map<Manager>(request);
        await managerRepository.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync();
        return mapper.Map<ManagerDto>(entity);
    }
}
