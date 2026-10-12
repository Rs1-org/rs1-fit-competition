using AutoMapper;
using Competition.Application.DTOs.CityDto;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using FluentValidation;
using MediatR;


namespace Competition.Application.Commands.CityCommands.Create;

public class CreateCityCommandHandler:IRequestHandler<CreateCityCommand,CityNameDto>
{
    private readonly ICity _city;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    public CreateCityCommandHandler(ICity city, IMapper mapper, IUnitOfWork unitOfWork)
    {
        _city = city;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<CityNameDto> Handle(CreateCityCommand request, CancellationToken cancellationToken)
    {
        var geoData = _mapper.Map<GeoData>(request);

        var city = _mapper.Map<City>(request);

        if (city == null)
        {
            throw new ValidationException("Couldn't create city");
        }

        // Attached by navigation: GeoData and the city are inserted in the same SaveChanges.
        city.GeoData = geoData;

        await _city.AddAsync(city, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CityNameDto>(city);

    }
    
}