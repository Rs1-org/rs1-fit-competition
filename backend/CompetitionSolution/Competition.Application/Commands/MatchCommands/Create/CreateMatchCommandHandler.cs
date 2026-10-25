using AutoMapper;
using Competition.Application.DTOs.MatchDto;
using Competition.Application.Helpers;
using FluentValidation;
using FluentValidation.Results;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using Competition.Domain.Entities.CompetitionEntities;
using Competition.Domain.Entities.MatchEntities;
using MediatR;
using NodaTime;

namespace Competition.Application.Commands.MatchCommands.Create;

public class CreateMatchCommandHandler : IRequestHandler<CreateMatchCommand, MatchDetailsDto>
{
    private readonly IMatch _match;
    private readonly ISelection _selections;
    private readonly IPublicIdLookup _lookup;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateMatchCommandHandler(IMatch match, ISelection selections, IPublicIdLookup lookup, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _match = match;
        _selections = selections;
        _lookup = lookup;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<MatchDetailsDto> Handle(CreateMatchCommand request, CancellationToken cancellationToken)
    {
        var match = _mapper.Map<Match>(request);
        match.Date = Instant.FromDateTimeUtc(DateTime.SpecifyKind(request.Date, DateTimeKind.Utc));

        // PublicId → internal Id; unknown references end as 404 before anything is saved.
        match.StatusId = await _lookup.GetRequiredIdAsync<MatchStatus>(request.StatusPublicId, cancellationToken);
        match.StadiumId = await _lookup.GetRequiredIdAsync<Stadium>(request.StadiumPublicId, cancellationToken);
        match.HomeSelectionId = await _lookup.GetOptionalIdAsync<Selection>(request.HomeSelectionPublicId, cancellationToken);
        match.AwaySelectionId = await _lookup.GetOptionalIdAsync<Selection>(request.AwaySelectionPublicId, cancellationToken);
        match.RoundId = await _lookup.GetOptionalIdAsync<Round>(request.RoundPublicId, cancellationToken);
        if (string.IsNullOrWhiteSpace(request.Slug))
            match.Slug = await GenerateSlugAsync(match, request.Date, cancellationToken);
        else if (await _match.SlugExistsAsync(request.Slug.Trim(), cancellationToken))
            throw new ValidationException([new ValidationFailure(nameof(request.Slug), "A match with this slug already exists.")]);
        else
            match.Slug = request.Slug.Trim();

        await _match.AddAsync(match, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _match.GetBySlugAsync(match.Slug!, cancellationToken);
        return _mapper.Map<MatchDetailsDto>(created);
    }

    /// <summary>"home-vs-away-2026-10-30" (or "match-2026-10-30" without teams), with "-2", "-3"... when taken.</summary>
    private async Task<string> GenerateSlugAsync(Match match, DateTime date, CancellationToken cancellationToken)
    {
        var home = match.HomeSelectionId is { } homeId ? (await _selections.GetByIdAsync(homeId, cancellationToken))?.Name : null;
        var away = match.AwaySelectionId is { } awayId ? (await _selections.GetByIdAsync(awayId, cancellationToken))?.Name : null;
        var teams = home is not null && away is not null ? $"{home} vs {away}" : "match";
        var baseSlug = SlugHelper.GenerateSlug($"{teams} {date:yyyy-MM-dd}");

        var slug = baseSlug;
        for (var suffix = 2; await _match.SlugExistsAsync(slug, cancellationToken); suffix++)
            slug = $"{baseSlug}-{suffix}";
        return slug;
    }
}
