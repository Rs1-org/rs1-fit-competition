using AutoMapper;
using Competition.Application.DTOs.StadiumDto;
using Competition.Application.Interfaces;
using Competition.Application.Services;
using Competition.Domain.Entities;
using Competition.Domain.Entities.MatchEntities;
using FluentValidation;
using MediatR;

namespace Competition.Application.Commands.StadiumCommands.Create;

public class CreateStadiumCommandHandler : IRequestHandler<CreateStadiumCommand, StadiumDetailsDto>
{
    private readonly IStadium _stadium;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageService _imageService;

    private readonly IPublicIdLookup _lookup;

    public CreateStadiumCommandHandler(IStadium stadium, IMapper mapper, IUnitOfWork unitOfWork, IImageService imageService, IPublicIdLookup lookup)
    {
        _lookup = lookup;
        _stadium = stadium;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _imageService = imageService;
    }

    public async Task<StadiumDetailsDto> Handle(CreateStadiumCommand request, CancellationToken cancellationToken)
    {
        // Unknown city → KeyNotFoundException (404) before the picture is uploaded.
        var cityId = await _lookup.GetRequiredIdAsync<City>(request.CityPublicId, cancellationToken);

        var geoData = _mapper.Map<GeoData>(request);

        var stadium = _mapper.Map<Stadium>(request);
        if (stadium == null)
        {
            throw new ValidationException("Couldn't create stadium");
        }

        // Attached by navigation: GeoData and the stadium are inserted in the same SaveChanges.
        stadium.GeoData = geoData;
        stadium.CityId = cityId;
        stadium.DateOfConstruction = DateTime.SpecifyKind(stadium.DateOfConstruction, DateTimeKind.Utc);

        if (request.Picture != null)
            stadium.PictureUrl = await _imageService.UploadImageAsync(request.Picture, cancellationToken);

        await _stadium.AddAsync(stadium, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _stadium.GetBySlugAsync(stadium.Slug!, cancellationToken);
        return _mapper.Map<StadiumDetailsDto>(created);
    }
}
