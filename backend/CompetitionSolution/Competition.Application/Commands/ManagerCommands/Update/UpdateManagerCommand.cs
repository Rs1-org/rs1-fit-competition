using Competition.Application.DTOs.PersonDto;
using MediatR;

namespace Competition.Application.Commands.ManagerCommands.Update;

public class UpdateManagerCommand(Guid id, ManagerDto dto) : IRequest<ManagerDto>
{
    public Guid Id { get; } = id;
    public ManagerDto Dto { get; } = dto;
}
