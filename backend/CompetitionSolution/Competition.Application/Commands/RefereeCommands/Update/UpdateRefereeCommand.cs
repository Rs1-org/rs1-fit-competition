using Competition.Application.DTOs.PersonDto;
using MediatR;

namespace Competition.Application.Commands.RefereeCommands.Update;

public class UpdateRefereeCommand(Guid id, RefereeDto dto) : IRequest<RefereeDto>
{
    public Guid Id { get; } = id;
    public RefereeDto Dto { get; } = dto;
}
