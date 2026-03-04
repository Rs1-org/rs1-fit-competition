using Competition.Application.DTOs.CityDto;
using MediatR;

public class DeleteCityCommands : IRequest<CityNameDto>
{
    public string Slug { get; set; }
}