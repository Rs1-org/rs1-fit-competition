using Competition.Application.Queries.City.GetAllByCountry;
using Competition.Application.Queries.City.GetBySlug;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
