using System.Text.Json.Serialization;
using AutoMapper;
using Competition.Application.DTOs.StatisticDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using Competition.Domain.Entities.MatchEntities;
using MediatR;
using NodaTime;

namespace Competition.Application.Commands.StatisticCommands;

public abstract class SaveStatisticCommand
{
    public TeamStatisticValues? Home { get; set; }
    public TeamStatisticValues? Away { get; set; }
}

public class CreateStatisticCommand : SaveStatisticCommand, IRequest<StatisticDto>
{
    public string MatchPublicId { get; set; } = string.Empty;
}

public class UpdateStatisticCommand : SaveStatisticCommand, IRequest<StatisticDto>
{
    [JsonIgnore] public string PublicId { get; set; } = string.Empty;
}

public record DeleteStatisticCommand(string PublicId) : IRequest<DeletedStatisticDto>;

internal static class StatisticCommandValues
{
    public static async Task ApplyAsync(Statistic statistic, SaveStatisticCommand request,
        IPublicIdLookup lookup, IMapper mapper, IClock clock, CancellationToken ct)
    {
        var match = statistic.Match;
        if (match.HomeSelectionId is null || match.AwaySelectionId is null || match.HomeSelectionId == match.AwaySelectionId)
            throw BusinessRule.Violation("MatchPublicId", "The match must have two distinct active selections before entering statistics.");
        var home = await ResolveSelectionAsync(match.HomeSelectionId.Value, statistic, lookup, ct);
        var away = await ResolveSelectionAsync(match.AwaySelectionId.Value, statistic, lookup, ct);
        ApplyTeam(statistic, home, request.Home!, mapper);
        ApplyTeam(statistic, away, request.Away!, mapper);
        // If the match teams changed, hide outdated rows instead of returning them as current statistics.
        foreach (var row in statistic.TeamStatistics.Where(t => t.SelectionId != home.Id && t.SelectionId != away.Id))
            row.DeletedAt = clock.GetCurrentInstant();
    }

    private static async Task<Selection> ResolveSelectionAsync(Guid id, Statistic statistic, IPublicIdLookup lookup, CancellationToken ct)
    {
        // Navigation selections are populated from the match repository; public lookup enforces active rows.
        var selection = statistic.Match.HomeSelectionId == id ? statistic.Match.HomeSelection : statistic.Match.AwaySelection;
        if (selection is null)
            throw BusinessRule.Violation("MatchPublicId", "Both match selections must exist and be active.");
        return await lookup.GetRequiredEntityAsync<Selection>(selection.PublicId, ct);
    }

    private static void ApplyTeam(Statistic statistic, Selection selection, TeamStatisticValues values, IMapper mapper)
    {
        var row = statistic.TeamStatistics.SingleOrDefault(x => x.SelectionId == selection.Id && x.DeletedAt == null);
        if (row is null)
        {
            row = new TeamStatistic { SelectionId = selection.Id, Selection = selection };
            statistic.TeamStatistics.Add(row);
        }
        mapper.Map(values, row);
    }
}

public class CreateStatisticCommandHandler(IStatistic statistics, IMatch matches, IPublicIdLookup lookup,
    IUnitOfWork unit, IMapper mapper, IClock clock) : IRequestHandler<CreateStatisticCommand, StatisticDto>
{
    public async Task<StatisticDto> Handle(CreateStatisticCommand request, CancellationToken cancellationToken)
    {
        var matchId = await lookup.GetIdByPublicIdAsync<Match>(request.MatchPublicId, cancellationToken)
            ?? throw BusinessRule.Violation(nameof(request.MatchPublicId), "Select an existing active match.");
        if (await statistics.ExistsForMatchAsync(matchId, cancellationToken))
            throw BusinessRule.Violation(nameof(request.MatchPublicId), "Statistics already exist for this match. Edit the existing record.");
        var match = await matches.GetByIdWithNavAsync(matchId, cancellationToken)
            ?? throw new KeyNotFoundException("Match not found.");
        var statistic = new Statistic { MatchId = matchId, Match = match };
        await StatisticCommandValues.ApplyAsync(statistic, request, lookup, mapper, clock, cancellationToken);
        await statistics.AddAsync(statistic, cancellationToken);
        await unit.SaveChangesAsync(cancellationToken);
        return mapper.Map<StatisticDto>(await statistics.GetDetailsAsync(statistic.PublicId, cancellationToken));
    }
}

public class UpdateStatisticCommandHandler(IStatistic statistics, IMatch matches, IPublicIdLookup lookup,
    IUnitOfWork unit, IMapper mapper, IClock clock) : IRequestHandler<UpdateStatisticCommand, StatisticDto>
{
    public async Task<StatisticDto> Handle(UpdateStatisticCommand request, CancellationToken cancellationToken)
    {
        var statistic = await statistics.GetDetailsAsync(request.PublicId, cancellationToken)
            ?? throw new KeyNotFoundException("Statistics not found.");
        statistic.Match = await matches.GetByIdWithNavAsync(statistic.MatchId, cancellationToken)
            ?? throw new KeyNotFoundException("Match not found.");
        await StatisticCommandValues.ApplyAsync(statistic, request, lookup, mapper, clock, cancellationToken);
        // The aggregate and its children are tracked; avoid marking the match/selection graph modified.
        await unit.SaveChangesAsync(cancellationToken);
        return mapper.Map<StatisticDto>(await statistics.GetDetailsAsync(statistic.PublicId, cancellationToken));
    }
}

public class DeleteStatisticCommandHandler(IStatistic statistics, IUnitOfWork unit, IClock clock)
    : IRequestHandler<DeleteStatisticCommand, DeletedStatisticDto>
{
    public async Task<DeletedStatisticDto> Handle(DeleteStatisticCommand request, CancellationToken cancellationToken)
    {
        var statistic = await statistics.GetDetailsAsync(request.PublicId, cancellationToken)
            ?? throw new KeyNotFoundException("Statistics not found.");
        // Set the soft-delete flag directly: tracked required children use restrictive foreign keys.
        statistic.DeletedAt = clock.GetCurrentInstant();
        foreach (var team in statistic.TeamStatistics)
            team.DeletedAt = statistic.DeletedAt;
        await unit.SaveChangesAsync(cancellationToken);
        return new DeletedStatisticDto(request.PublicId);
    }
}
