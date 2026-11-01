using Competition.Application.Common.Validation;
using Competition.Application.DTOs.MatchDto;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.MatchEntities;
using FluentValidation;
using MediatR;

namespace Competition.Application.Lineups;

public record GetMatchLineupQuery(string PublicId) : IRequest<MatchLineupDto>;

public class GetMatchLineupQueryValidator : AbstractValidator<GetMatchLineupQuery>
{
    public GetMatchLineupQueryValidator() => RuleFor(x => x.PublicId).MustBeValidPublicId();
}

public class GetMatchLineupQueryHandler(IMatchLineup lineups, IPublicIdLookup lookup)
    : IRequestHandler<GetMatchLineupQuery, MatchLineupDto>
{
    public async Task<MatchLineupDto> Handle(GetMatchLineupQuery request, CancellationToken cancellationToken)
    {
        var id = await lookup.GetRequiredIdAsync<Match>(request.PublicId, cancellationToken);
        var match = await lineups.GetMatchAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Match with public id '{request.PublicId}' not found.");
        return MatchLineupMapper.ToDto(match);
    }
}

public static class MatchLineupMapper
{
    public static MatchLineupDto ToDto(Match match)
    {
        var rows = match.Lineups
            .OrderByDescending(x => x.IsStarter)
            .ThenBy(x => x.ShirtNumber ?? int.MaxValue)
            .ThenBy(x => x.Player?.LastName)
            .Select(ToPlayer)
            .ToList();

        return new MatchLineupDto
        {
            MatchPublicId = match.PublicId,
            RefereePublicId = match.Referee?.PublicId,
            RefereeName = match.Referee == null ? null : $"{match.Referee.FirstName} {match.Referee.LastName}".Trim(),
            HomeFormationPublicId = match.HomeFormation?.PublicId,
            HomeFormationCode = match.HomeFormation?.Code,
            AwayFormationPublicId = match.AwayFormation?.PublicId,
            AwayFormationCode = match.AwayFormation?.Code,
            Home = rows.Where(x => x.SelectionPublicId == match.HomeSelection?.PublicId).ToList(),
            Away = rows.Where(x => x.SelectionPublicId == match.AwaySelection?.PublicId).ToList()
        };
    }

    private static LineupPlayerDto ToPlayer(MatchLineup row) => new()
    {
        PublicId = row.PublicId,
        PlayerPublicId = row.Player?.PublicId ?? string.Empty,
        PlayerName = row.Player == null ? string.Empty : $"{row.Player.FirstName} {row.Player.LastName}".Trim(),
        SelectionPublicId = row.Selection?.PublicId ?? string.Empty,
        PositionPublicId = row.Position?.PublicId,
        PositionName = row.Position?.Name,
        SubPositionPublicId = row.SubPosition?.PublicId,
        SubPositionName = row.SubPosition?.Name,
        IsStarter = row.IsStarter,
        ShirtNumber = row.ShirtNumber
    };
}
