using Competition.Application.DTOs.CityDto;
using Competition.Domain.Entities;
using MediatR;

namespace Competition.Application.Commands.CityCommands.Create;

public class CreateCityCommand : IRequest<CityNameDto>
{
    public string? Name { get; set; }
    public int Population {  get; set; }
    public bool IsCapital {  get; set; }
    public string CountryPublicId { get; set; } = string.Empty;
    
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string? Description {  get; set; }
}