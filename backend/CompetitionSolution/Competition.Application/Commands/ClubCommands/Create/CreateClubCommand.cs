using Competition.Application.DTOs.ClubDto;
using Competition.Domain.Entities;
using MediatR;

namespace Competition.Application.Commands.ClubCommands.Create;

public class CreateClubCommand : IRequest<ClubResponseDto>
{
    public string? Name { get; set; }
    public string? FoundingDate { get; set; }
    public Guid? CityId { get; set; }
    public ClubAge? ClubAge { get; set; }
    public Guid? StadiumId { get; set; }
}
