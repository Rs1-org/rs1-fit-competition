using Competition.Application.DTOs.CountryDto;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Application.Queries.Country.GetBySlug
{
    public class GetCountryBySlugCommand : IRequest<CountryDetailsDto>
    {
        public string Slug { get; set; }
    }
}
