using AutoMapper;
using Competition.Application.DTOs.MatchDto;
using Competition.Application.Interfaces;
using MediatR;
using NodaTime;

namespace Competition.Application.Commands.MatchCommands.Update;

public class UpdateMatchCommandHandler : IRequestHandler<UpdateMatchCommand, MatchDetailsDto>
{
    private readonly IMatch _match;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateMatchCommandHandler(IMatch match, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _match = match;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<MatchDetailsDto> Handle(UpdateMatchCommand request, CancellationToken cancellationToken)
    {
        var match = await _match.GetBySlugAsync(request.Slug, cancellationToken)
            ?? throw new KeyNotFoundException($"Match with slug '{request.Slug}' not found.");

        match.Date = Instant.FromDateTimeUtc(DateTime.SpecifyKind(request.Date, DateTimeKind.Utc));
        match.FollowersCount = request.FollowersCount;
        match.HomeScore = request.HomeScore;
        match.AwayScore = request.AwayScore;
        match.HalfHomeScore = request.HalfHomeScore;
        match.HalfAwayScore = request.HalfAwayScore;
        match.IsPostponed = request.IsPostponed;
        match.StatusId = request.StatusId;
        match.StadiumId = request.StadiumId;

        await _match.UpdateAsync(match, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _match.GetBySlugAsync(match.Slug!, cancellationToken);
        return _mapper.Map<MatchDetailsDto>(updated);
    }
}
