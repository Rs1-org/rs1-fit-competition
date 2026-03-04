using Competition.Application.Commands.CityCommands.Create;
using Competition.Application.Commands.CityCommands.Update;
using Competition.Application.Commands.CountryCommands.Create;
using Competition.Application.Commands.CountryCommands.Delete;
using Competition.Application.Commands.CountryCommands.Update;
using Competition.Application.Queries.City.GetAll;
using Competition.Application.Queries.Country.GetBySlug;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountryController : ControllerBase
    {
        private readonly IMediator _mediator;
        public CountryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("createCountry")]
        public async Task<IActionResult> CreateCountry([FromBody] CreateCountryCommands request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpDelete("deleteCountry/{slug}")]
        public async Task<IActionResult> DeleteCountry(string slug, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteCountryCommands { Slug = slug });
            return Ok(result);
        }

        [HttpPut("updateCountry/{slug}")]
        public async Task<IActionResult> UpdateCountry(string slug, [FromBody] UpdateCountryCommands request, CancellationToken cancellationToken)
        {
            request.Slug=slug;
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpGet("getCountry/{slug}")]
        public async Task<IActionResult> GetCountry(string slug, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetCountryBySlugCommand { Slug=slug});
            return Ok(result);
        }


        [HttpGet("getCountries")]
        public async Task<IActionResult> GetCountries(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAllCountriesCommand());
            return Ok(result);
        }
    }
}
