using Competition.Application.DTOs.CityDto;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Application.Queries.City.GetBySlug
{
    public class GetCityBySlugCommand : IRequest<CityDetailsDto>
    {
        public string Slug { get; set; }
    }
}
