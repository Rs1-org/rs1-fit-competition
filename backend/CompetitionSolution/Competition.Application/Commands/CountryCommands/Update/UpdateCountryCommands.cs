using Competition.Application.DTOs.CountryDto;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Competition.Application.Commands.CountryCommands.Update
{
    public class UpdateCountryCommands : IRequest<CountryNameDto>
    {
        [JsonIgnore]
        public string Slug { get; set; }
        public string? Name { get; set; }
        public string? PictureUrl { get; set; }
    }
}
