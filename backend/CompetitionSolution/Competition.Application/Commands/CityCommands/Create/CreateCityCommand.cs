using System.ComponentModel.DataAnnotations.Schema;
using Competition.Application.DTOs.CityDto;
using Competition.Domain.Entities;
using MediatR;

namespace Competition.Application.Commands.CityCommands.Create;

public class CreateCityCommand : IRequest<CityNameDto>
{
    public string? Name { get; set; }
    public int Population {  get; set; }
    public bool IsCapital {  get; set; }
    public Guid CountryId {  get; set; }
    
    [Column(TypeName = "decimal(9,6)")]
    public decimal Latitude { get; set; }
    [Column(TypeName = "decimal(9,6)")]
    public decimal Longitude { get; set; }
    public string? Description {  get; set; }
}