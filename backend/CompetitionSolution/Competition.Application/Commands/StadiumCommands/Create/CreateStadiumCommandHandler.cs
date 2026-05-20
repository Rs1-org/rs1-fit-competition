using AutoMapper;
using Competition.Application.DTOs.StadiumDto;
using Competition.Application.Interfaces;
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

    public CreateStadiumCommandHandler(IStadium stadium, IGeoData geoData, IMapper mapper, IUnitOfWork unitOfWork)
    {
        _stadium = stadium;
        _geoData = geoData;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
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

        await _stadium.AddAsync(stadium);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<StadiumDetailsDto>(stadium);
    }
}
