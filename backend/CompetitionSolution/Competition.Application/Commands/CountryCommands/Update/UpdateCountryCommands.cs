using Competition.Application.DTOs.CountryDto;
using MediatR;
using Microsoft.AspNetCore.Http;
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
        public string Slug { get; set; } = string.Empty;
        public string? Name { get; set; }
        public IFormFile? Picture { get; set; }
    }
}
