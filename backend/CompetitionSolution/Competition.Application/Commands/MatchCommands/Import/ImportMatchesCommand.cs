using Competition.Application.DTOs.ImportExportDto;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Competition.Application.Commands.MatchCommands.Import;

/// <summary>
/// Imports matches from a .csv/.xlsx file. Required columns: Date (yyyy-MM-dd HH:mm, UTC), Status, Stadium,
/// HomeTeam, AwayTeam (existing names; a team may be empty). Optional: Competition, Season, Round (all three
/// or none), HomeScore, AwayScore, HalfHomeScore, HalfAwayScore, IsPostponed, FollowersCount. Other columns are ignored.
/// </summary>
public class ImportMatchesCommand : IRequest<ImportResultDto>
{
    public IFormFile? File { get; set; }
}

/// <summary>One parsed row; property names are the column names so validation errors point at the right column.</summary>
public record MatchImportRow(string Status, string Stadium, string HomeTeam, string AwayTeam,
    int? HomeScore, int? AwayScore, int? HalfHomeScore, int? HalfAwayScore, int FollowersCount);
