using Competition.Application.DTOs.CityDto;
using Competition.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Application.DTOs.CountryDto
{
    public class CountryDetailsDto
    {
        public string? Name { get; set; }
        public string? Slug { get; set; }
        public List<CityNameDto>? Cities { get; set; }
        //public List<Competition>? Competitions { get; set; }
        //public List<Person>? Persons { get; set; }
        public string? PictureUrl { get; set; }
    }
}
