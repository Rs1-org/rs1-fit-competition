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
    private readonly IPublicIdLookup _lookup;

    public CreateCityCommandHandler(ICity city, IMapper mapper, IUnitOfWork unitOfWork, IPublicIdLookup lookup)
    {
        _city = city;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _lookup = lookup;
    }

    public async Task<CityNameDto> Handle(CreateCityCommand request, CancellationToken cancellationToken)
    {
        // Unknown country → KeyNotFoundException (404) before anything is created.
        var countryId = await _lookup.GetRequiredIdAsync<Country>(request.CountryPublicId, cancellationToken);

        var geoData = _mapper.Map<GeoData>(request);

        var city = _mapper.Map<City>(request);

        if (city == null)
        {
            throw new ValidationException("Couldn't create city");
        }

        // Attached by navigation: GeoData and the city are inserted in the same SaveChanges.
        city.GeoData = geoData;
        city.CountryId = countryId;

        await _city.AddAsync(city, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CityNameDto>(city);

    }
    
}