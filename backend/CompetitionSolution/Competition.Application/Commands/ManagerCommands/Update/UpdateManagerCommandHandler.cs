using AutoMapper;
using Competition.Application.DTOs.PersonDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Features.ManagerCommands.Update;

public class UpdateManagerCommandHandler(IManager managerRepository, IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<UpdateManagerCommand, ManagerDto>
{
    public async Task<ManagerDto> Handle(UpdateManagerCommand request, CancellationToken cancellationToken)
    {
        var manager = await managerRepository.GetActiveByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Manager with id {request.Id} not found");

        mapper.Map(request, manager);
        await unitOfWork.SaveChangesAsync();
        return mapper.Map<ManagerDto>(manager);
    }
}
