using System.Text.Json.Serialization;
using Competition.Application.DTOs.RoundDto;
using MediatR;

namespace Competition.Application.Commands.RoundCommands.Update;

public class UpdateRoundCommand : IRequest<RoundDetailsDto>
{
    [JsonIgnore]
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Number { get; set; }
}
