using Competition.Application.DTOs.CountryDto;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Application.Commands.CountryCommands.Delete
{
    public class DeleteCountryCommands : IRequest<CountryNameDto>
    {
        public string Slug { get; set; }
    }
}
