using System.Text.Json.Serialization;
using Competition.Application.DTOs.SeasonDto;
using MediatR;

namespace Competition.Application.Commands.SeasonCommands.Update;

public class UpdateSeasonCommand : IRequest<SeasonDetailsDto>
{
    [JsonIgnore]
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsCurrent { get; set; }
}
