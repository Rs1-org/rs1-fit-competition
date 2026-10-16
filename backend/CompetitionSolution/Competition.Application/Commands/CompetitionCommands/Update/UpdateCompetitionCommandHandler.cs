using AutoMapper;
using Competition.Application.DTOs.CompetitionDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using MediatR;

namespace Competition.Application.Commands.CompetitionCommands.Update;

public class UpdateCompetitionCommandHandler : IRequestHandler<UpdateCompetitionCommand, CompetitionDetailsDto>
{
    private readonly ICompetition _competition;
    private readonly IPublicIdLookup _lookup;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateCompetitionCommandHandler(ICompetition competition, IPublicIdLookup lookup, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _competition = competition;
        _lookup = lookup;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CompetitionDetailsDto> Handle(UpdateCompetitionCommand request, CancellationToken cancellationToken)
    {
        var competition = await _competition.GetBySlugAsync(request.Slug, cancellationToken)
            ?? throw new KeyNotFoundException($"Competition with slug '{request.Slug}' not found.");

        var name = request.Name.Trim();
        var slug = SlugHelper.GenerateSlug(name);
        if (await _competition.SlugExistsAsync(slug, competition.Id, cancellationToken))
            throw BusinessRule.Violation(nameof(request.Name), $"A competition named '{name}' (or a very similar name) already exists.");
        var countryId = await _lookup.GetOptionalIdAsync<Country>(request.CountryPublicId, cancellationToken);

        competition.Name = name;
        competition.Slug = slug;
        competition.ShortName = request.ShortName?.Trim();
        competition.Description = request.Description?.Trim();
        competition.CountryId = countryId;
        competition.Country = null;

        await _competition.UpdateAsync(competition, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _competition.GetBySlugAsync(competition.Slug, cancellationToken);
        return _mapper.Map<CompetitionDetailsDto>(updated);
    }
}
