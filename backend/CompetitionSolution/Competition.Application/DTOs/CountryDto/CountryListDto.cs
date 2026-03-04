using Competition.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Application.DTOs.CountryDto
{
    public class CountryListDto
    {
        public string? Name { get; set; }
        public string? PictureUrl { get; set; }
    }
}
