using Competition.Application.DTOs.CityDto;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Competition.Application.Commands.CityCommands.Update
{
    public class UpdateCityCommands : IRequest<CityNameDto>
    {
        [JsonIgnore]
        public string Slug { get; set; } = string.Empty;
        public string? Name { get; set; }
        public int Population { get; set; }
        public bool IsCapital { get; set; }

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? Description { get; set; }
    }
}
