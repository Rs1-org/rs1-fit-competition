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
        public string PublicId { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Slug { get; set; }
        public int Population { get; set; }
        public bool IsCapital { get; set; }
        public string? CountryName { get; set; }
    }
}
