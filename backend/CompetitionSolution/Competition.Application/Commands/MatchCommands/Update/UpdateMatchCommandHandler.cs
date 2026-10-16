using AutoMapper;
using Competition.Application.DTOs.MatchDto;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using Competition.Domain.Entities.CompetitionEntities;
using Competition.Domain.Entities.MatchEntities;
using MediatR;
using NodaTime;

namespace Competition.Application.Commands.MatchCommands.Update;

public class UpdateMatchCommandHandler : IRequestHandler<UpdateMatchCommand, MatchDetailsDto>
{
    private readonly IMatch _match;
    private readonly IPublicIdLookup _lookup;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateMatchCommandHandler(IMatch match, IPublicIdLookup lookup, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _match = match;
        _lookup = lookup;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<MatchDetailsDto> Handle(UpdateMatchCommand request, CancellationToken cancellationToken)
    {
        var match = await _match.GetBySlugAsync(request.Slug, cancellationToken)
            ?? throw new KeyNotFoundException($"Match with slug '{request.Slug}' not found.");

        // PublicId → internal Id; unknown references end as 404 before anything is saved.
        var statusId = await _lookup.GetRequiredIdAsync<MatchStatus>(request.StatusPublicId, cancellationToken);
        var stadiumId = await _lookup.GetRequiredIdAsync<Stadium>(request.StadiumPublicId, cancellationToken);
        var homeSelectionId = await _lookup.GetOptionalIdAsync<Selection>(request.HomeSelectionPublicId, cancellationToken);
        var awaySelectionId = await _lookup.GetOptionalIdAsync<Selection>(request.AwaySelectionPublicId, cancellationToken);
        var roundId = await _lookup.GetOptionalIdAsync<Round>(request.RoundPublicId, cancellationToken);

        match.Date = Instant.FromDateTimeUtc(DateTime.SpecifyKind(request.Date, DateTimeKind.Utc));
        match.FollowersCount = request.FollowersCount;
        match.HomeScore = request.HomeScore;
        match.AwayScore = request.AwayScore;
        match.HalfHomeScore = request.HalfHomeScore;
        match.HalfAwayScore = request.HalfAwayScore;
        match.IsPostponed = request.IsPostponed;
        match.StatusId = statusId;
        match.StadiumId = stadiumId;
        match.HomeSelectionId = homeSelectionId;
        match.AwaySelectionId = awaySelectionId;
        match.RoundId = roundId;

        await _match.UpdateAsync(match, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _match.GetBySlugAsync(match.Slug!, cancellationToken);
        return _mapper.Map<MatchDetailsDto>(updated);
    }
}
