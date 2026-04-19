using Competition.Application.DTOs.CityDto;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Competition.Application.Commands.CityCommands.Update
{
    public class UpdateCityCommands : IRequest<CityNameDto>
    {
        [JsonIgnore]
        public string Slug { get; set; }
        public string? Name { get; set; }
        public int Population { get; set; }
        public bool IsCapital { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal Latitude { get; set; }
        [Column(TypeName = "decimal(9,6)")]
        public decimal Longitude { get; set; }
        public string? Description { get; set; }
    }
}
