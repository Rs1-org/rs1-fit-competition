using Competition.Application.Common.ImportExport;
using Competition.Application.Common.Models;
using Competition.Application.Common.Validation;
using Competition.Application.Queries.Match.Export;
using Competition.Application.Queries.Match.GetAll;
using FluentValidation;

namespace Competition.Application.Queries.Match;

public class GetAllMatchesQueryValidator : AbstractValidator<GetAllMatchesQuery>
{
    public GetAllMatchesQueryValidator()
    {
        RuleFor(x => x.SortBy).MustBeSortKey(SortKeys.Match);
        RuleFor(x => x.SortDir).MustBeSortDirection();
        RuleFor(x => x.Phase).MustBeSortKey(MatchPhases.All);
    }
}

public class ExportMatchesQueryValidator : AbstractValidator<ExportMatchesQuery>
{
    public ExportMatchesQueryValidator()
    {
        RuleFor(x => x.Format).MustBeExportFormat();
        RuleFor(x => x.SortBy).MustBeSortKey(SortKeys.Match);
        RuleFor(x => x.SortDir).MustBeSortDirection();
        RuleFor(x => x.Phase).MustBeSortKey(MatchPhases.All);
        RuleFor(x => x.StatusPublicId).MustBeValidPublicId().When(x => x.StatusPublicId is not null);
        RuleFor(x => x.StadiumPublicId).MustBeValidPublicId().When(x => x.StadiumPublicId is not null);
        RuleFor(x => x.CompetitionPublicId).MustBeValidPublicId().When(x => x.CompetitionPublicId is not null);
        RuleFor(x => x.DateTo)
            .GreaterThanOrEqualTo(x => x.DateFrom)
            .When(x => x.DateFrom.HasValue && x.DateTo.HasValue)
            .WithMessage("'Date to' must be on or after 'Date from'.");
    }
}
