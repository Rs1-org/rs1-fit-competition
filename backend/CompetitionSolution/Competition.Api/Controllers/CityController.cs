using Competition.Application.Commands.CityCommands.Create;
using Competition.Application.Commands.CityCommands.Update;
using Competition.Application.Queries.City.GetAll;
using Competition.Application.Queries.City.GetAllByCountry;
using Competition.Application.Queries.City.GetBySlug;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class CityController : ControllerBase
{
    private readonly IMediator _mediator;
    public CityController(IMediator mediator)
    {
        _mediator=mediator;
    }

    [HttpPost("createCity")]
    public async Task<IActionResult>CreateCity([FromBody]CreateCityCommand request, CancellationToken cancellationToken)
    {
        var result= await _mediator.Send(request);
        return Ok(result);
    }

    [HttpDelete("deleteCity/{id}")]
    public async Task<IActionResult> DeleteCity(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteCityCommands { Id = id });
        return Ok(result);
    }

    [HttpPut("updateCity/{slug}")]
    public async Task<IActionResult> UpdateCity(string slug, [FromBody] UpdateCityCommands request, CancellationToken cancellationToken)
    {
        request.Slug = slug;
        var result = await _mediator.Send(request);
        return Ok(result);
    }

    [HttpGet("getCities")]
    public async Task<IActionResult> GetCities(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllCitiesCommand());
        return Ok(result);
    }

    [HttpGet("getCity/{slug}")]
    public async Task<IActionResult> GetCity(string slug, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCityBySlugCommand { Slug = slug });
        return Ok(result);
    }

    [HttpGet("getCitiesByCountry/{countryId}")]
    public async Task<IActionResult> GetCitiesByCountry(Guid countryId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllCitiesByCountryCommand { CountryId = countryId });
        return Ok(result);
    }
}