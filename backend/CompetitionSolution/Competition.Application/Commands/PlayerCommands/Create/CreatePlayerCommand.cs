using Competition.Application.DTOs.PersonDto;
using MediatR;

namespace Competition.Application.Commands.PlayerCommands.Create;

public class CreatePlayerCommand(PlayerDto dto) : IRequest<PlayerDto>
{
    public PlayerDto Dto { get; } = dto;
}
