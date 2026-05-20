using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Competition.Application.DTOs.StadiumDto;
using MediatR;

namespace Competition.Application.Commands.StadiumCommands.Update;

public class UpdateStadiumCommand : IRequest<StadiumDetailsDto>
{
    [JsonIgnore]
    public string? Slug { get; set; }
    public string? Name { get; set; }
    public int Capacity { get; set; }
    public DateTime DateOfConstruction { get; set; }
    public string? PictureUrl { get; set; }
    public Guid CityId { get; set; }
    
    [Column(TypeName = "decimal(9,6)")]
    public decimal Latitude { get; set; }
    [Column(TypeName = "decimal(9,6)")]
    public decimal Longitude { get; set; }
    public string? GeoDescription { get; set; }
}
