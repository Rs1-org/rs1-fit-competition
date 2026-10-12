using Competition.Application.DTOs.SeasonDto;
using MediatR;

namespace Competition.Application.Commands.SeasonCommands.Create;

public class CreateSeasonCommand : IRequest<SeasonDetailsDto>
{
    public Guid CompetitionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsCurrent { get; set; }
}
