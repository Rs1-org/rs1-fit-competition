using Competition.Application.Queries.City.GetAll;
using Competition.Application.Queries.City.GetAllByCountry;
using Competition.Application.Queries.City.GetBySlug;
using Competition.Application.Queries.Country.GetBySlug;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Application.Queries.Country
{
    public class GetCountryBySlugValidator : AbstractValidator<GetCountryBySlugCommand>
    {
        public GetCountryBySlugValidator()
        {
            RuleFor(s => s.Slug).NotEmpty();
        }
    }
}
