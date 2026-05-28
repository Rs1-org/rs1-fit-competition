using Competition.Application.DTOs.PersonDto;
using MediatR;

namespace Competition.Application.Commands.PlayerCommands.Update;

public class UpdatePlayerCommand(Guid id, PlayerDto dto) : IRequest<PlayerDto>
{
    public Guid Id { get; } = id;
    public PlayerDto Dto { get; } = dto;
}
