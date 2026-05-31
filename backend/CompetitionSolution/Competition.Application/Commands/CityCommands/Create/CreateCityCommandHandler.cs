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
    private readonly IGeoData _geoData;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    public CreateCityCommandHandler(ICity city, IGeoData geoData, IMapper mapper, IUnitOfWork unitOfWork)
    {
        _city = city;
        _geoData = geoData;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<CityNameDto> Handle(CreateCityCommand request, CancellationToken cancellationToken)
    {
        var geoData = _mapper.Map<GeoData>(request);
        await _geoData.AddAsync(geoData, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var city = _mapper.Map<City>(request);

        if (city == null)
        {
            throw new ValidationException("Couldn't create city");
        }

        city.GeoDataId = geoData.Id;

        await _city.AddAsync(city, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CityNameDto>(city);

    }
    
}