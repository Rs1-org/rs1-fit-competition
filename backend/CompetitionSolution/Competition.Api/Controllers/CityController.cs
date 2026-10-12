using Competition.Application.Commands.CityCommands.Create;
using Competition.Application.Commands.CityCommands.Delete;
using Competition.Application.Commands.CityCommands.Update;
using Competition.Application.Queries.City.GetAll;
using Competition.Application.Queries.City.GetAllByCountry;
using Competition.Application.Queries.City.GetBySlug;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPost("createCity")]
    public async Task<IActionResult>CreateCity([FromBody]CreateCityCommand request, CancellationToken cancellationToken)
    {
        var result= await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("deleteCity/{id}")]
    public async Task<IActionResult> DeleteCity(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteCityCommands { Id = id }, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPut("updateCity/{slug}")]
    public async Task<IActionResult> UpdateCity(string slug, [FromBody] UpdateCityCommands request, CancellationToken cancellationToken)
    {
        request.Slug = slug;
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("getCities")]
    public async Task<IActionResult> GetCities(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? name = null,
        [FromQuery] Guid? countryId = null,
        [FromQuery] bool? isCapital = null,
        [FromQuery] int? minPopulation = null,
        [FromQuery] int? maxPopulation = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAllCitiesCommand
        {
            Page = page,
            PageSize = pageSize,
            Name = name,
            CountryId = countryId,
            IsCapital = isCapital,
            MinPopulation = minPopulation,
            MaxPopulation = maxPopulation
        }, cancellationToken);
        return Ok(result);
    }

    [HttpGet("getCity/{slug}")]
    public async Task<IActionResult> GetCity(string slug, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCityBySlugCommand { Slug = slug }, cancellationToken);
        return Ok(result);
    }

    [HttpGet("getCitiesByCountry/{countryId}")]
    public async Task<IActionResult> GetCitiesByCountry(Guid countryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAllCitiesByCountryCommand { CountryId = countryId, Page = page, PageSize = pageSize }, cancellationToken);
        return Ok(result);
    }
}