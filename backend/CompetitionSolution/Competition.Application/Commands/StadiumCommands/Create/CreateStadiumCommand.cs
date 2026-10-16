using Competition.Application.DTOs.StadiumDto;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Competition.Application.Commands.StadiumCommands.Create;

public class CreateStadiumCommand : IRequest<StadiumDetailsDto>
{
    public string? Name { get; set; }
    public int Capacity { get; set; }
    public DateTime DateOfConstruction { get; set; }
    public IFormFile? Picture { get; set; }
    public string CityPublicId { get; set; } = string.Empty;

    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string? GeoDescription { get; set; }
}
