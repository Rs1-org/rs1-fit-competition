using AutoMapper;
using Competition.Application.DTOs.SeasonDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.CompetitionEntities;
using MediatR;
using NodaTime;

namespace Competition.Application.Commands.SeasonCommands.Create;

public class CreateSeasonCommandHandler : IRequestHandler<CreateSeasonCommand, SeasonDetailsDto>
{
    private readonly ISeason _season;
    private readonly ICompetition _competition;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateSeasonCommandHandler(ISeason season, ICompetition competition, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _season = season;
        _competition = competition;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SeasonDetailsDto> Handle(CreateSeasonCommand request, CancellationToken cancellationToken)
    {
        var competition = await _competition.GetByIdAsync(request.CompetitionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Competition with ID '{request.CompetitionId}' not found.");

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
