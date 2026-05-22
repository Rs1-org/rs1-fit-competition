using System.ComponentModel.DataAnnotations.Schema;
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
    public Guid CityId { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal Latitude { get; set; }
    [Column(TypeName = "decimal(9,6)")]
    public decimal Longitude { get; set; }
    public string? GeoDescription { get; set; }
}
