using Competition.Application.DTOs.CityDto;
using MediatR;

public class DeleteCityCommands : IRequest<CityNameDto>
{
    public Guid Id { get; set; }
}