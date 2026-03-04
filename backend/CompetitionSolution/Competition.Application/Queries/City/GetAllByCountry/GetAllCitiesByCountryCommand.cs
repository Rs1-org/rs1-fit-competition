using Competition.Application.DTOs.CityDto;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Application.Queries.City.GetAllByCountry
{
    public class GetAllCitiesByCountryCommand : IRequest<List<CityListDto>>
    {
        public Guid CountryId { get; set; }
    }
}
