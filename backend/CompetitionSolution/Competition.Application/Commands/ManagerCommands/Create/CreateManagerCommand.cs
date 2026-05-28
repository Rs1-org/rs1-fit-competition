using Competition.Application.DTOs.PersonDto;
using MediatR;

namespace Competition.Application.Commands.ManagerCommands.Create;

public class CreateManagerCommand(ManagerDto dto) : IRequest<ManagerDto>
{
    public ManagerDto Dto { get; } = dto;
}
