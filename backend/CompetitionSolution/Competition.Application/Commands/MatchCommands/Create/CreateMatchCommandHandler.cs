using AutoMapper;
using Competition.Application.DTOs.MatchDto;
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
    private readonly IPublicIdLookup _lookup;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateMatchCommandHandler(IMatch match, IPublicIdLookup lookup, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _match = match;
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

        await _match.AddAsync(match, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _match.GetBySlugAsync(match.Slug!, cancellationToken);
        return _mapper.Map<MatchDetailsDto>(created);
    }
}
