using Competition.Application.Commands.CityCommands.Create;
using Competition.Application.Commands.CityCommands.Import;
using Competition.Application.Queries.City.Export;
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
    [HttpDelete("deleteCity/{publicId}")]
    public async Task<IActionResult> DeleteCity(string publicId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteCityCommands { PublicId = publicId }, cancellationToken);
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
        [FromQuery] string? countryPublicId = null,
        [FromQuery] bool? isCapital = null,
        [FromQuery] int? minPopulation = null,
        [FromQuery] int? maxPopulation = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAllCitiesCommand
        {
            Page = page,
            PageSize = pageSize,
            Name = name,
            CountryPublicId = countryPublicId,
            IsCapital = isCapital,
            MinPopulation = minPopulation,
            MaxPopulation = maxPopulation,
            SortBy = sortBy,
            SortDir = sortDir
        }, cancellationToken);
        return Ok(result);
    }

    [HttpGet("getCity/{slug}")]
    public async Task<IActionResult> GetCity(string slug, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCityBySlugCommand { Slug = slug }, cancellationToken);
        return Ok(result);
    }

    [HttpGet("getCitiesByCountry/{countryPublicId}")]
    public async Task<IActionResult> GetCitiesByCountry(string countryPublicId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAllCitiesByCountryCommand { CountryPublicId = countryPublicId, Page = page, PageSize = pageSize }, cancellationToken);
        return Ok(result);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] ExportCitiesQuery query, CancellationToken cancellationToken)
    {
        var file = await _mediator.Send(query, cancellationToken);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("import")]
    [RequestSizeLimit(2 * 1024 * 1024 + 64 * 1024)]
    public async Task<IActionResult> Import([FromForm] ImportCitiesCommand request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }
}
