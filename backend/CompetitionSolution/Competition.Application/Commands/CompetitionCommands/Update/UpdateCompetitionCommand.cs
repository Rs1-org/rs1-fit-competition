using System.Text.Json.Serialization;
using Competition.Application.DTOs.CompetitionDto;
using MediatR;

namespace Competition.Application.Commands.CompetitionCommands.Update;

public class UpdateCompetitionCommand : IRequest<CompetitionDetailsDto>
{
    [JsonIgnore]
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? Description { get; set; }
    public string? CountryPublicId { get; set; }
}
