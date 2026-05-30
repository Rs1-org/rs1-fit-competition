using AutoMapper;
using Competition.Application.DTOs.StadiumDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Application.Services;
using Competition.Domain.Entities.MatchEntities;
using MediatR;

namespace Competition.Application.Commands.StadiumCommands.Update;

public class UpdateStadiumCommandHandler : IRequestHandler<UpdateStadiumCommand, StadiumDetailsDto>
{
    private readonly IMapper _mapper;
    private readonly IStadium _stadium;
    private readonly IGeoData _geoData;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageService _imageService;

    public UpdateStadiumCommandHandler(IMapper mapper, IStadium stadium, IGeoData geoData, IUnitOfWork unitOfWork, IImageService imageService)
    {
        _mapper = mapper;
        _stadium = stadium;
        _geoData = geoData;
        _unitOfWork = unitOfWork;
        _imageService = imageService;
    }

    public async Task<StadiumDetailsDto> Handle(UpdateStadiumCommand request, CancellationToken cancellationToken)
    {
        var stadium = await _stadium.GetBySlugAsync(request.Slug!, cancellationToken);
        if (stadium == null)
        {
            throw new KeyNotFoundException($"Couldn't find any stadium with slug {request.Slug}");
        }

        stadium.Name = request.Name;
        stadium.Capacity = request.Capacity;
        stadium.DateOfConstruction = DateTime.SpecifyKind(request.DateOfConstruction, DateTimeKind.Utc);
        stadium.CityId = request.CityId;
        stadium.Slug = SlugHelper.GenerateSlug(request.Name!);

        if (request.Picture != null)
            stadium.PictureUrl = await _imageService.UploadImageAsync(request.Picture);

        if (stadium.GeoData != null)
        {
            stadium.GeoData.Latitude = request.Latitude;
            stadium.GeoData.Longitude = request.Longitude;
            stadium.GeoData.Description = request.GeoDescription;
        }

        await _stadium.UpdateAsync(stadium, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<StadiumDetailsDto>(stadium);
    }
}
