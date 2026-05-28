using Competition.Application.DTOs.PersonDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Commands.ManagerCommands.Update;

public class UpdateManagerCommandHandler(IManager managerRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateManagerCommand, ManagerDto>
{
    public async Task<ManagerDto> Handle(UpdateManagerCommand request, CancellationToken cancellationToken)
    {
        var manager = await managerRepository.GetActiveByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Manager with id {request.Id} not found");

        PersonMappingHelper.Apply(request.Dto, manager);
        await unitOfWork.SaveChangesAsync();
        return PersonMappingHelper.ToDto(manager);
    }
}
