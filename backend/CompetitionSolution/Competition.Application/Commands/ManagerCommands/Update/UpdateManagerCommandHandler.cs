using AutoMapper;
using Competition.Application.DTOs.PersonDto;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.Person;
using MediatR;

namespace Competition.Application.Commands.ManagerCommands.Update;

public class UpdateManagerCommandHandler(IManager managerRepository, IUnitOfWork unitOfWork, IMapper mapper, IPublicIdLookup lookup)
    : IRequestHandler<UpdateManagerCommand, ManagerDto>
{
    public async Task<ManagerDto> Handle(UpdateManagerCommand request, CancellationToken cancellationToken)
    {
        var id = await lookup.GetRequiredIdAsync<Manager>(request.PublicId, cancellationToken);
        var manager = await managerRepository.GetActiveByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Manager with public id {request.PublicId} not found");

        mapper.Map(request, manager);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return mapper.Map<ManagerDto>(manager);
    }
}
