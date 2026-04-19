using Competition.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Application.DTOs.CityDto
{
    public class CityListDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public bool IsCapital { get; set; }
        public string? CountryName { get; set; }

    }
}
