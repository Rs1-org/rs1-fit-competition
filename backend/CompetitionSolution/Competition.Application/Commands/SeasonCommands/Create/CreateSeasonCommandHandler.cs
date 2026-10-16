using AutoMapper;
using Competition.Application.DTOs.SeasonDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.CompetitionEntities;
using CompetitionEntity = Competition.Domain.Entities.CompetitionEntities.Competition;
using MediatR;
using NodaTime;

namespace Competition.Application.Commands.SeasonCommands.Create;

public class CreateSeasonCommandHandler : IRequestHandler<CreateSeasonCommand, SeasonDetailsDto>
{
    private readonly ISeason _season;
    private readonly IPublicIdLookup _lookup;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateSeasonCommandHandler(ISeason season, IPublicIdLookup lookup, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _season = season;
        _lookup = lookup;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SeasonDetailsDto> Handle(CreateSeasonCommand request, CancellationToken cancellationToken)
    {
        var competition = await _lookup.GetRequiredEntityAsync<CompetitionEntity>(request.CompetitionPublicId, cancellationToken);

        var name = request.Name.Trim();
        var slug = SlugHelper.GenerateSlug($"{competition.Slug} {name.Replace('/', ' ')}");
        if (await _season.SlugExistsAsync(slug, null, cancellationToken))
            throw BusinessRule.Violation(nameof(request.Name), $"{competition.Name} already has a season named '{name}' (or a very similar name).");

        if (request.IsCurrent)
            await _season.UnsetCurrentAsync(competition.Id, null, cancellationToken);

        var season = new Season
        {
            CompetitionId = competition.Id,
            Name = name,
            Slug = slug,
            StartDate = LocalDate.FromDateOnly(request.StartDate),
            EndDate = LocalDate.FromDateOnly(request.EndDate),
            IsCurrent = request.IsCurrent
        };

        await _season.AddAsync(season, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _season.GetBySlugAsync(season.Slug, cancellationToken);
        return _mapper.Map<SeasonDetailsDto>(created);
    }
}
