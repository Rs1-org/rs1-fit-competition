using Competition.Application.Common.ImportExport;
using Competition.Application.Queries.Country.Export;
using Competition.Application.Queries.Country.GetAll;
using Competition.Application.Queries.Country.GetBySlug;
using Competition.Application.Common.Models;
using Competition.Application.Common.Validation;
using FluentValidation;

namespace Competition.Application.Queries.Country
{
    public class GetCountryBySlugValidator : AbstractValidator<GetCountryBySlugCommand>
    {
        public GetCountryBySlugValidator()
        {
            RuleFor(s => s.Slug).NotEmpty();
        }
    }

    public class GetAllCountriesValidator : AbstractValidator<GetAllCountriesCommand>
    {
        public GetAllCountriesValidator()
        {
            RuleFor(s => s.Page).GreaterThanOrEqualTo(1);
            RuleFor(s => s.PageSize).InclusiveBetween(1, 100);
            RuleFor(s => s.Name).MaximumLength(100);
            RuleFor(s => s.Slug).MaximumLength(100);
            RuleFor(s => s.SortBy).MustBeSortKey(SortKeys.Country);
            RuleFor(s => s.SortDir).MustBeSortDirection();
            RuleFor(s => s.CreatedTo)
                .GreaterThanOrEqualTo(s => s.CreatedFrom)
                .When(s => s.CreatedFrom.HasValue && s.CreatedTo.HasValue)
                .WithMessage("'Created to' must be on or after 'Created from'.");
        }
    }

    public class ExportCountriesValidator : AbstractValidator<ExportCountriesQuery>
    {
        public ExportCountriesValidator()
        {
            RuleFor(s => s.Format).MustBeExportFormat();
            RuleFor(s => s.Name).MaximumLength(100);
            RuleFor(s => s.Slug).MaximumLength(100);
            RuleFor(s => s.SortBy).MustBeSortKey(SortKeys.Country);
            RuleFor(s => s.SortDir).MustBeSortDirection();
            RuleFor(s => s.CreatedTo)
                .GreaterThanOrEqualTo(s => s.CreatedFrom)
                .When(s => s.CreatedFrom.HasValue && s.CreatedTo.HasValue)
                .WithMessage("'Created to' must be on or after 'Created from'.");
        }
    }
}
