using Competition.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Application.DTOs.CityDto
{
    public class CityDetailsDto
    {
        public string? Name { get; set; }
        public int Population { get; set; }
        public bool IsCapital { get; set; }
        public string Slug { get; set; }

        // public List<Club>? Clubs {  get; set; }
        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }
        public string? Description { get; set; }
        public string CountryName { get; set; }
        public string CountryPublicId { get; set; } = string.Empty;

    }
}
