using AutoMapper;
using Competition.Application.DTOs.CityDto;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using FluentValidation;
using MediatR;


namespace Competition.Application.Commands.CityCommands.Create;

public class CreateCityCommandHandler(ICity city, IGeoData data, IMapper mapper, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateCityCommand, CityNameDto>
{
    public async Task<CityNameDto> Handle(CreateCityCommand request, CancellationToken cancellationToken)
    {
        var geoData = mapper.Map<GeoData>(request);
        await data.AddAsync(geoData);
        await unitOfWork.SaveChangesAsync(cancellationToken); 

        var city1 = mapper.Map<City>(request);

        if (city1 == null)
        {
            throw new ValidationException("Couldn't create city");
        }

        city1.GeoDataId = geoData.Id;

        await city.AddAsync(city1);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<CityNameDto>(city1);

    }
    
}