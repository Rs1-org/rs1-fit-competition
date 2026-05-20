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
    private readonly IGeoData _geoData;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageService _imageService;

    public CreateStadiumCommandHandler(IStadium stadium, IGeoData geoData, IMapper mapper, IUnitOfWork unitOfWork, IImageService imageService)
    {
        _stadium = stadium;
        _geoData = geoData;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _imageService = imageService;
    }

    public async Task<StadiumDetailsDto> Handle(CreateStadiumCommand request, CancellationToken cancellationToken)
    {
        var geoData = _mapper.Map<GeoData>(request);
        await _geoData.AddAsync(geoData);
        await _unitOfWork.SaveChangesAsync();

        var stadium = _mapper.Map<Stadium>(request);
        if (stadium == null)
        {
            throw new ValidationException("Couldn't create stadium");
        }

        stadium.GeoDataId = geoData.Id;
        stadium.DateOfConstruction = DateTime.SpecifyKind(stadium.DateOfConstruction, DateTimeKind.Utc);

        if (request.Picture != null)
            stadium.PictureUrl = await _imageService.UploadImageAsync(request.Picture);

        await _stadium.AddAsync(stadium);
        await _unitOfWork.SaveChangesAsync();

        var created = await _stadium.GetBySlugAsync(stadium.Slug!);
        return _mapper.Map<StadiumDetailsDto>(created);
    }
}
