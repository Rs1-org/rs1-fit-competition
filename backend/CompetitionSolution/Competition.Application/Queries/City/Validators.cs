using Competition.Application.Queries.City.GetAll;
using Competition.Application.Queries.City.GetAllByCountry;
using Competition.Application.Queries.City.GetBySlug;
using FluentValidation;

namespace Competition.Application.Queries.City
{
    public class GetAllCitiesByCountryValidator : AbstractValidator<GetAllCitiesByCountryCommand>
    {
        public GetAllCitiesByCountryValidator()
        {
            RuleFor(s => s.CountryId).NotEmpty();
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
            RuleFor(s => s.CountryId).NotEqual(Guid.Empty).When(s => s.CountryId.HasValue);
            RuleFor(s => s.MinPopulation).GreaterThanOrEqualTo(0).When(s => s.MinPopulation.HasValue);
            RuleFor(s => s.MaxPopulation).GreaterThanOrEqualTo(0).When(s => s.MaxPopulation.HasValue);
            RuleFor(s => s.MaxPopulation)
                .GreaterThanOrEqualTo(s => s.MinPopulation)
                .When(s => s.MinPopulation.HasValue && s.MaxPopulation.HasValue)
                .WithMessage("'Max population' must be greater than or equal to 'Min population'.");
        }
    }
}
