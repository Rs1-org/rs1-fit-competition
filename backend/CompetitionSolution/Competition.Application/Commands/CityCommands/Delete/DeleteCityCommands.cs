using Competition.Application.DTOs.CityDto;
using MediatR;

namespace Competition.Application.Commands.CityCommands.Delete;

public class DeleteCityCommands : IRequest<CityNameDto>
{
    public Guid Id { get; set; }
}