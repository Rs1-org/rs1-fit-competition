using Competition.Application.DTOs.PersonDto;
using MediatR;

namespace Competition.Application.Commands.RefereeCommands.Create;

public class CreateRefereeCommand(RefereeDto dto) : IRequest<RefereeDto>
{
    public RefereeDto Dto { get; } = dto;
}
