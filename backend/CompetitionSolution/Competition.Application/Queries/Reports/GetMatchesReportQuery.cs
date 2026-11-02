using Competition.Application.Common.Validation;
using Competition.Application.DTOs.ImportExportDto;
using Competition.Application.DTOs.ReportDto;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.MatchEntities;
using FluentValidation;
using MediatR;
using CompetitionEntity = Competition.Domain.Entities.CompetitionEntities.Competition;
using StadiumEntity = Competition.Domain.Entities.MatchEntities.Stadium;

namespace Competition.Application.Queries.Reports;

/// <summary>PDF report of matches in a date range (RS1 item 12, parameters: start and end date), with the match list filters.</summary>
public class GetMatchesReportQuery : IRequest<ExportFileDto>
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? StatusPublicId { get; set; }
    public string? StadiumPublicId { get; set; }
    public string? CompetitionPublicId { get; set; }
    public bool? IsPostponed { get; set; }
}

public class GetMatchesReportQueryValidator : AbstractValidator<GetMatchesReportQuery>
{
    public GetMatchesReportQueryValidator()
    {
        RuleFor(x => x.DateFrom).NotNull();
        RuleFor(x => x.DateTo).NotNull();
        RuleFor(x => x.DateTo)
            .GreaterThanOrEqualTo(x => x.DateFrom)
            .When(x => x.DateFrom.HasValue && x.DateTo.HasValue)
            .WithMessage("'Date to' must be on or after 'Date from'.");
        RuleFor(x => x.StatusPublicId).MustBeValidPublicId().When(x => x.StatusPublicId is not null);
        RuleFor(x => x.StadiumPublicId).MustBeValidPublicId().When(x => x.StadiumPublicId is not null);
        RuleFor(x => x.CompetitionPublicId).MustBeValidPublicId().When(x => x.CompetitionPublicId is not null);
    }
}

public class GetMatchesReportQueryHandler(IMatch matches, IPublicIdLookup lookup, IPdfReportService pdf)
    : IRequestHandler<GetMatchesReportQuery, ExportFileDto>
{
    /// <summary>A longer list is cut off, and the report says so.</summary>
    public const int MaxRows = 1000;

    public async Task<ExportFileDto> Handle(GetMatchesReportQuery request, CancellationToken cancellationToken)
    {
        var dateFrom = request.DateFrom!.Value;
        var dateTo = request.DateTo!.Value;

        // Same rule as the list: an unknown filter matches nothing (empty report, not a 404).
        var statusId = await lookup.GetIdByPublicIdAsync<MatchStatus>(request.StatusPublicId, cancellationToken);
        var stadiumId = await lookup.GetIdByPublicIdAsync<StadiumEntity>(request.StadiumPublicId, cancellationToken);
        var competitionId = await lookup.GetIdByPublicIdAsync<CompetitionEntity>(request.CompetitionPublicId, cancellationToken);
        var unknownFilter = (request.StatusPublicId is not null && statusId is null)
                            || (request.StadiumPublicId is not null && stadiumId is null)
                            || (request.CompetitionPublicId is not null && competitionId is null);

        var report = new MatchesReportDto { DateFrom = dateFrom, DateTo = dateTo };
        if (!unknownFilter)
        {
            var paged = await matches.GetPagedWithFiltersAsync(
                1, MaxRows,
                statusId, stadiumId, null,
                request.IsPostponed,
                dateFrom, dateTo,
                null, competitionId,
                cancellationToken);

            report.TotalCount = paged.TotalCount;
            report.Rows = paged.Items.Select(m => new MatchesReportRowDto
            {
                Date = m.Date.ToDateTimeUtc(),
                HomeName = m.HomeSelection?.Name ?? "TBD",
                AwayName = m.AwaySelection?.Name ?? "TBD",
                HomeScore = m.HomeScore,
                AwayScore = m.AwayScore,
                StatusName = m.Status?.Name,
                IsPostponed = m.IsPostponed,
                StadiumName = m.Stadium?.Name,
                CompetitionName = m.Round?.Season?.Competition?.Name
            }).ToList();

            // Filters are public ids; every row shares the filtered value, so the first row names it.
            var first = paged.Items.FirstOrDefault();
            var filters = new List<string>();
            if (statusId.HasValue && first?.Status is not null) filters.Add($"Status: {first.Status.Name}");
            if (stadiumId.HasValue && first?.Stadium is not null) filters.Add($"Stadium: {first.Stadium.Name}");
            if (competitionId.HasValue && first?.Round?.Season?.Competition is not null)
                filters.Add($"Competition: {first.Round.Season.Competition.Name}");
            if (request.IsPostponed.HasValue) filters.Add(request.IsPostponed.Value ? "Postponed only" : "Not postponed");
            report.Filters = filters;
        }

        return ReportFile.Pdf(pdf.MatchesReport(report), $"matches-report-{dateFrom:yyyyMMdd}-{dateTo:yyyyMMdd}");
    }
}
