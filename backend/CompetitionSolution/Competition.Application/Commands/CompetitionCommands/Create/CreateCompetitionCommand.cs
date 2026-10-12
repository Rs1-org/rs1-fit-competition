using Competition.Application.DTOs.CompetitionDto;
using MediatR;

namespace Competition.Application.Commands.CompetitionCommands.Create;

public class CreateCompetitionCommand : IRequest<CompetitionDetailsDto>
{
    public string Name { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? Description { get; set; }
    public Guid? CountryId { get; set; }
}
