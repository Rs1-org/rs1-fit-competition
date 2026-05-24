using Competition.Application.Commands.CityCommands.Create;
using Competition.Application.Commands.CityCommands.Update;
using Competition.Application.Commands.CountryCommands.Create;
using Competition.Application.Commands.CountryCommands.Delete;
using Competition.Application.Commands.CountryCommands.Update;
using Competition.Application.Queries.Country.GetAll;
using Competition.Application.Queries.Country.GetBySlug;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateCountry([FromForm] CreateCountryCommands request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteCountry(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteCountryCommands { Id = id });
            return Ok(result);
        }

        [Authorize]
        [HttpPut("update/{slug}")]
        public async Task<IActionResult> UpdateCountry(string slug, [FromForm] UpdateCountryCommands request, CancellationToken cancellationToken)
        {
            request.Slug=slug;
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpGet("get/{slug}")]
        public async Task<IActionResult> GetCountry(string slug, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetCountryBySlugCommand { Slug=slug});
            return Ok(result);
        }


        [HttpGet("getAll")]
        public async Task<IActionResult> GetCountries(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAllCountriesCommand());
            return Ok(result);
        }
    }
}
