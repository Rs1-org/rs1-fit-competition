using Competition.Application.DTOs.CountryDto;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Application.Commands.CountryCommands.Create
{
    public class CreateCountryCommands : IRequest<CountryNameDto>
    {
        public string? Name { get; set; }
        public string? PictureUrl { get; set; }
    }
}
