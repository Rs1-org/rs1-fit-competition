using AutoMapper;
using Competition.Application.DTOs.CompetitionDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using MediatR;
using CompetitionEntity = Competition.Domain.Entities.CompetitionEntities.Competition;

namespace Competition.Application.Commands.CompetitionCommands.Create;

public class CreateCompetitionCommandHandler : IRequestHandler<CreateCompetitionCommand, CompetitionDetailsDto>
{
    private readonly ICompetition _competition;
    private readonly ICountry _country;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateCompetitionCommandHandler(ICompetition competition, ICountry country, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _competition = competition;
        _country = country;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CompetitionDetailsDto> Handle(CreateCompetitionCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        var slug = SlugHelper.GenerateSlug(name);
        if (await _competition.SlugExistsAsync(slug, null, cancellationToken))
            throw BusinessRule.Violation(nameof(request.Name), $"A competition named '{name}' (or a very similar name) already exists.");

        if (request.CountryId.HasValue && await _country.GetByIdAsync(request.CountryId.Value, cancellationToken) is null)
            throw new KeyNotFoundException($"Country with ID '{request.CountryId}' not found.");

        var competition = new CompetitionEntity
        {
            Name = name,
            Slug = slug,
            ShortName = request.ShortName?.Trim(),
            Description = request.Description?.Trim(),
            CountryId = request.CountryId
        };

        await _competition.AddAsync(competition, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _competition.GetBySlugAsync(competition.Slug, cancellationToken);
        return _mapper.Map<CompetitionDetailsDto>(created);
    }
}
