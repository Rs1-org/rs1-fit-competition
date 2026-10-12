using AutoMapper;
using Competition.Application.DTOs.SeasonDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using MediatR;
using NodaTime;

namespace Competition.Application.Commands.SeasonCommands.Update;

public class UpdateSeasonCommandHandler : IRequestHandler<UpdateSeasonCommand, SeasonDetailsDto>
{
    private readonly ISeason _season;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateSeasonCommandHandler(ISeason season, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _season = season;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SeasonDetailsDto> Handle(UpdateSeasonCommand request, CancellationToken cancellationToken)
    {
        var season = await _season.GetBySlugAsync(request.Slug, cancellationToken)
            ?? throw new KeyNotFoundException($"Season with slug '{request.Slug}' not found.");

        var name = request.Name.Trim();
        var slug = SlugHelper.GenerateSlug($"{season.Competition!.Slug} {name.Replace('/', ' ')}");
        if (await _season.SlugExistsAsync(slug, season.Id, cancellationToken))
            throw BusinessRule.Violation(nameof(request.Name), $"{season.Competition.Name} already has a season named '{name}' (or a very similar name).");

        if (request.IsCurrent && !season.IsCurrent)
            await _season.UnsetCurrentAsync(season.CompetitionId, season.Id, cancellationToken);

        season.Name = name;
        season.Slug = slug;
        season.StartDate = LocalDate.FromDateOnly(request.StartDate);
        season.EndDate = LocalDate.FromDateOnly(request.EndDate);
        season.IsCurrent = request.IsCurrent;

        await _season.UpdateAsync(season, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _season.GetBySlugAsync(season.Slug, cancellationToken);
        return _mapper.Map<SeasonDetailsDto>(updated);
    }
}
