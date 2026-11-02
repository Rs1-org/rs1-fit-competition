using Competition.Application.Commands.MatchCommands.Validators;
using Competition.Application.Common.ImportExport;
using Competition.Application.DTOs.ImportExportDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.CompetitionEntities;
using MediatR;
using NodaTime;
using MatchEntity = Competition.Domain.Entities.MatchEntities.Match;

namespace Competition.Application.Commands.MatchCommands.Import;

public class ImportMatchesCommandHandler : IRequestHandler<ImportMatchesCommand, ImportResultDto>
{
    private static readonly string[] RequiredColumns = ["Date", "Status", "Stadium", "HomeTeam", "AwayTeam"];
    private static readonly MatchImportRowValidator RowValidator = new();

    private readonly IMatch _match;
    private readonly IMatchStatus _status;
    private readonly IStadium _stadium;
    private readonly ISelection _selection;
    private readonly IRound _round;
    private readonly ITabularFileService _files;
    private readonly IUnitOfWork _unitOfWork;

    public ImportMatchesCommandHandler(IMatch match, IMatchStatus status, IStadium stadium, ISelection selection, IRound round,
        ITabularFileService files, IUnitOfWork unitOfWork)
    {
        _match = match;
        _status = status;
        _stadium = stadium;
        _selection = selection;
        _round = round;
        _files = files;
        _unitOfWork = unitOfWork;
    }

    public async Task<ImportResultDto> Handle(ImportMatchesCommand request, CancellationToken cancellationToken)
    {
        var (records, result) = await ImportFile.ReadAsync(_files, request.File!, RequiredColumns, cancellationToken);
        if (!result.Success) return result;

        var statuses = ByName(await _status.GetAllAsync(cancellationToken), s => s.Name, s => s.Id);
        var stadiums = ByName(await _stadium.GetAllAsync(cancellationToken), s => s.Name, s => s.Id);
        var teams = ByName(await _selection.GetAllAsync(cancellationToken), s => s.Name, s => s.Id);
        var rounds = ByName(await _round.GetAllWithSeasonAsync(cancellationToken), RoundKey, r => r.Id);

        // The same two teams at the same time is the same match (existing matches and earlier rows).
        var taken = (await _match.GetAllAsync(cancellationToken))
            .Select(m => (m.HomeSelectionId, m.AwaySelectionId, m.Date))
            .ToHashSet();
        var slugs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var toAdd = new List<MatchEntity>();

        for (var i = 0; i < records.Count; i++)
        {
            var errorsBefore = result.Errors.Count;
            var reader = new ImportRowReader(records[i], ImportFile.RowNumber(i), result.Errors);
            var date = reader.DateTimeUtc("Date");
            var homeScore = reader.OptionalInt("HomeScore");
            var awayScore = reader.OptionalInt("AwayScore");
            var halfHomeScore = reader.OptionalInt("HalfHomeScore");
            var halfAwayScore = reader.OptionalInt("HalfAwayScore");
            var followers = reader.OptionalInt("FollowersCount");
            var postponed = reader.Bool("IsPostponed");
            if (result.Errors.Count > errorsBefore) continue;

            var row = new MatchImportRow(reader.Text("Status"), reader.Text("Stadium"), reader.Text("HomeTeam"), reader.Text("AwayTeam"),
                homeScore, awayScore, halfHomeScore, halfAwayScore, followers ?? 0);
            var validation = RowValidator.Validate(row);
            if (!validation.IsValid) { reader.AddErrors(validation); continue; }

            if (!statuses.TryGetValue(row.Status, out var statusId))
                reader.AddError("Status", $"Status '{row.Status}' does not exist.");
            if (!stadiums.TryGetValue(row.Stadium, out var stadiumId))
                reader.AddError("Stadium", $"Stadium '{row.Stadium}' does not exist. Import or create it first.");
            var homeId = TeamId(row.HomeTeam, "HomeTeam", teams, reader);
            var awayId = TeamId(row.AwayTeam, "AwayTeam", teams, reader);
            var roundId = RoundId(reader, rounds);
            if (result.Errors.Count > errorsBefore) continue;

            var instant = Instant.FromDateTimeUtc(date!.Value);
            if (homeId is not null && awayId is not null && !taken.Add((homeId, awayId, instant)))
            {
                reader.AddError("Date", $"{row.HomeTeam} vs {row.AwayTeam} on {date:yyyy-MM-dd HH:mm} already exists.");
                continue;
            }

            toAdd.Add(new MatchEntity
            {
                Date = instant,
                Slug = await UniqueSlugAsync(row, date.Value, homeId, awayId, slugs, cancellationToken),
                StatusId = statusId,
                StadiumId = stadiumId,
                HomeSelectionId = homeId,
                AwaySelectionId = awayId,
                RoundId = roundId,
                HomeScore = row.HomeScore,
                AwayScore = row.AwayScore,
                HalfHomeScore = row.HalfHomeScore,
                HalfAwayScore = row.HalfAwayScore,
                IsPostponed = postponed!.Value,
                FollowersCount = row.FollowersCount
            });
        }

        // All-or-nothing: one bad row means nothing is saved.
        if (!result.Success) return result;

        foreach (var match in toAdd)
            await _match.AddAsync(match, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        result.Imported = toAdd.Count;
        return result;
    }

    private static Guid? TeamId(string name, string column, Dictionary<string, Guid> teams, ImportRowReader reader)
    {
        if (name.Length == 0) return null;
        if (teams.TryGetValue(name, out var id)) return id;
        reader.AddError(column, $"Team '{name}' does not exist. Import or create it first.");
        return null;
    }

    private static Guid? RoundId(ImportRowReader reader, Dictionary<string, Guid> rounds)
    {
        var (competition, season, round) = (reader.Text("Competition"), reader.Text("Season"), reader.Text("Round"));
        if (competition.Length == 0 && season.Length == 0 && round.Length == 0) return null;
        if (rounds.TryGetValue(Key(competition, season, round), out var id)) return id;
        reader.AddError("Round", $"Round '{round}' of {competition} {season} does not exist.");
        return null;
    }

    /// <summary>Same pattern as a created match: "home-vs-away-2026-10-30", with "-2", "-3"... when taken.</summary>
    private async Task<string> UniqueSlugAsync(MatchImportRow row, DateTime date, Guid? homeId, Guid? awayId,
        HashSet<string> slugs, CancellationToken cancellationToken)
    {
        var teams = homeId is not null && awayId is not null ? $"{row.HomeTeam} vs {row.AwayTeam}" : "match";
        var baseSlug = SlugHelper.GenerateSlug($"{teams} {date:yyyy-MM-dd}");

        var slug = baseSlug;
        for (var suffix = 2; slugs.Contains(slug) || await _match.SlugExistsAsync(slug, cancellationToken); suffix++)
            slug = $"{baseSlug}-{suffix}";
        slugs.Add(slug);
        return slug;
    }

    private static Dictionary<string, Guid> ByName<T>(IEnumerable<T> items, Func<T, string?> name, Func<T, Guid> id) =>
        items.Where(item => !string.IsNullOrWhiteSpace(name(item)))
            .GroupBy(item => name(item)!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => id(g.First()), StringComparer.OrdinalIgnoreCase);

    private static string RoundKey(Round round) =>
        Key(round.Season?.Competition?.Name ?? string.Empty, round.Season?.Name ?? string.Empty, round.Name);

    private static string Key(string competition, string season, string round) => $"{competition.Trim()}|{season.Trim()}|{round.Trim()}";
}
