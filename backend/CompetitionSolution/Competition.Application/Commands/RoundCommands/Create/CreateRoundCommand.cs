using Competition.Application.DTOs.RoundDto;
using MediatR;

namespace Competition.Application.Commands.RoundCommands.Create;

public class CreateRoundCommand : IRequest<RoundDetailsDto>
{
    public Guid SeasonId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Number { get; set; }
}
