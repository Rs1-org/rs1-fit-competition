using Competition.Application.Common.ImportExport;
using Competition.Application.Common.Models;
using Competition.Application.DTOs.ImportExportDto;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.MatchEntities;
using MediatR;
using CompetitionEntity = Competition.Domain.Entities.CompetitionEntities.Competition;
using StadiumEntity = Competition.Domain.Entities.MatchEntities.Stadium;

namespace Competition.Application.Queries.Match.Export;

public class ExportMatchesQueryHandler : IRequestHandler<ExportMatchesQuery, ExportFileDto>
{
    /// <summary>Same columns the import reads, so an exported file can be edited and imported back.</summary>
    public static readonly string[] Columns =
    [
        "PublicId", "Date", "Status", "Stadium", "HomeTeam", "AwayTeam", "Competition", "Season", "Round",
        "HomeScore", "AwayScore", "HalfHomeScore", "HalfAwayScore", "IsPostponed", "FollowersCount"
    ];

    private readonly IMatch _match;
    private readonly IPublicIdLookup _lookup;
    private readonly ITabularFileService _files;

    public ExportMatchesQueryHandler(IMatch match, IPublicIdLookup lookup, ITabularFileService files)
    {
        _match = match;
        _lookup = lookup;
        _files = files;
    }

    public async Task<ExportFileDto> Handle(ExportMatchesQuery request, CancellationToken cancellationToken)
    {
        var format = ImportExportFormat.Parse(request.Format);

        // Same rule as the list: an unknown filter matches nothing (empty file, not a 404).
        var statusId = await _lookup.GetIdByPublicIdAsync<MatchStatus>(request.StatusPublicId, cancellationToken);
        var stadiumId = await _lookup.GetIdByPublicIdAsync<StadiumEntity>(request.StadiumPublicId, cancellationToken);
        var competitionId = await _lookup.GetIdByPublicIdAsync<CompetitionEntity>(request.CompetitionPublicId, cancellationToken);
        var unknownFilter = (request.StatusPublicId is not null && statusId is null)
                            || (request.StadiumPublicId is not null && stadiumId is null)
                            || (request.CompetitionPublicId is not null && competitionId is null);

        var matches = unknownFilter
            ? []
            : await _match.GetFilteredListAsync(
                new MatchListFilter(statusId, stadiumId, null, request.IsPostponed, request.DateFrom, request.DateTo,
                    null, competitionId, request.Phase, new SortSpec(request.SortBy, request.SortDir)),
                ImportExportFormat.MaxExportRows, cancellationToken);

        var rows = matches.Select(m => (IReadOnlyList<object?>)
        [
            m.PublicId, m.Date.ToDateTimeUtc().ToString(ImportRowReader.DateTimeFormat), m.Status?.Name, m.Stadium?.Name,
            m.HomeSelection?.Name, m.AwaySelection?.Name,
            m.Round?.Season?.Competition?.Name, m.Round?.Season?.Name, m.Round?.Name,
            m.HomeScore, m.AwayScore, m.HalfHomeScore, m.HalfAwayScore, m.IsPostponed, m.FollowersCount
        ]);

        return ImportExportFormat.ToFile(_files.Write(Columns, rows, format, "Matches"), format, "matches");
    }
}
