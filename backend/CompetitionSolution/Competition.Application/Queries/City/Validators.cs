using Competition.Application.Queries.City.GetAll;
using Competition.Application.Queries.City.GetAllByCountry;
using Competition.Application.Queries.City.GetBySlug;
using Competition.Application.Common.Validation;
using Competition.Application.Common.Models;
using Competition.Application.Common.ImportExport;
using Competition.Application.Queries.City.Export;
using FluentValidation;

namespace Competition.Application.Queries.City
{
    public class GetAllCitiesByCountryValidator : AbstractValidator<GetAllCitiesByCountryCommand>
    {
        public GetAllCitiesByCountryValidator()
        {
            RuleFor(s => s.CountryPublicId).MustBeValidPublicId();
        }
    }

    public class GetCityBySlugValidator : AbstractValidator<GetCityBySlugCommand>
    {
        public GetCityBySlugValidator()
        {
            RuleFor(s => s.Slug).NotEmpty();
        }
    }

    public class GetAllCitiesValidator : AbstractValidator<GetAllCitiesCommand>
    {
        public GetAllCitiesValidator()
        {
            RuleFor(s => s.Page).GreaterThanOrEqualTo(1);
            RuleFor(s => s.PageSize).InclusiveBetween(1, 100);
            RuleFor(s => s.Name).MaximumLength(100);
            RuleFor(s => s.SortBy).MustBeSortKey(SortKeys.City);
            RuleFor(s => s.SortDir).MustBeSortDirection();
            RuleFor(s => s.CountryPublicId).MustBeValidPublicId().When(s => s.CountryPublicId is not null);
            RuleFor(s => s.MinPopulation).GreaterThanOrEqualTo(0).When(s => s.MinPopulation.HasValue);
            RuleFor(s => s.MaxPopulation).GreaterThanOrEqualTo(0).When(s => s.MaxPopulation.HasValue);
            RuleFor(s => s.MaxPopulation)
                .GreaterThanOrEqualTo(s => s.MinPopulation)
                .When(s => s.MinPopulation.HasValue && s.MaxPopulation.HasValue)
                .WithMessage("'Max population' must be greater than or equal to 'Min population'.");
        }
    }

    public class ExportCitiesValidator : AbstractValidator<ExportCitiesQuery>
    {
        public ExportCitiesValidator()
        {
            RuleFor(s => s.Format).MustBeExportFormat();
            RuleFor(s => s.Name).MaximumLength(100);
            RuleFor(s => s.SortBy).MustBeSortKey(SortKeys.City);
            RuleFor(s => s.SortDir).MustBeSortDirection();
            RuleFor(s => s.CountryPublicId).MustBeValidPublicId().When(s => s.CountryPublicId is not null);
            RuleFor(s => s.MinPopulation).GreaterThanOrEqualTo(0).When(s => s.MinPopulation.HasValue);
            RuleFor(s => s.MaxPopulation)
                .GreaterThanOrEqualTo(s => s.MinPopulation)
                .When(s => s.MinPopulation.HasValue && s.MaxPopulation.HasValue)
                .WithMessage("'Max population' must be greater than or equal to 'Min population'.");
        }
    }
}
