using System.Text.Json.Serialization;
using Competition.Application.DTOs.ClubDto;
using Competition.Domain.Entities;
using MediatR;

namespace Competition.Application.Commands.ClubCommands.Update;

public class UpdateClubCommand : IRequest<ClubResponseDto>
{
    [JsonIgnore]
    public string? Slug { get; set; }
    public string? Name { get; set; }
    public string? FoundingDate { get; set; }
    public Guid? CityId { get; set; }
    public ClubAge? ClubAge { get; set; }
    public Guid? StadiumId { get; set; }
}
