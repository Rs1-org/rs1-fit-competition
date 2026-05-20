using AutoMapper;
using Competition.Application.DTOs.StadiumDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.MatchEntities;
using MediatR;

namespace Competition.Application.Commands.StadiumCommands.Update;

public class UpdateStadiumCommandHandler : IRequestHandler<UpdateStadiumCommand, StadiumDetailsDto>
{
    private readonly IMapper _mapper;
    private readonly IStadium _stadium;
    private readonly IGeoData _geoData;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateStadiumCommandHandler(IMapper mapper, IStadium stadium, IGeoData geoData, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _stadium = stadium;
        _geoData = geoData;
        _unitOfWork = unitOfWork;
    }

    public async Task<StadiumDetailsDto> Handle(UpdateStadiumCommand request, CancellationToken cancellationToken)
    {
        var stadium = await _stadium.GetBySlugAsync(request.Slug!);
        if (stadium == null)
        {
            throw new KeyNotFoundException($"Couldn't find any stadium with slug {request.Slug}");
        }

        // Map request to stadium entity
        stadium.Name = request.Name;
        stadium.Capacity = request.Capacity;
        stadium.DateOfConstruction = request.DateOfConstruction;
        stadium.PictureUrl = request.PictureUrl;
        stadium.CityId = request.CityId;
        stadium.Slug = SlugHelper.GenerateSlug(request.Name!);

        if (stadium.GeoData != null)
        {
            stadium.GeoData.Latitude = request.Latitude;
            stadium.GeoData.Longitude = request.Longitude;
            stadium.GeoData.Description = request.GeoDescription;
        }

        await _stadium.UpdateAsync(stadium);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<StadiumDetailsDto>(stadium);
    }
}
