using Competition.Application.DTOs.CityDto;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Application.Queries.City.GetAll
{
    public class GetAllCitiesCommand : IRequest<List<CityListDto>>
    {

    }
}
