using Competition.Application.Commands.CityCommands.Create;
using Competition.Application.Commands.CountryCommands.Import;
using Competition.Application.Queries.Country.Export;
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

        [Authorize(Roles = "Admin,Moderator")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateCountry([FromForm] CreateCountryCommands request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(request, cancellationToken);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{publicId}")]
        public async Task<IActionResult> DeleteCountry(string publicId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteCountryCommands { PublicId = publicId }, cancellationToken);
            return Ok(result);
        }

        [Authorize(Roles = "Admin,Moderator")]
        [HttpPut("update/{slug}")]
        public async Task<IActionResult> UpdateCountry(string slug, [FromForm] UpdateCountryCommands request, CancellationToken cancellationToken)
        {
            request.Slug=slug;
            var result = await _mediator.Send(request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("get/{slug}")]
        public async Task<IActionResult> GetCountry(string slug, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetCountryBySlugCommand { Slug=slug}, cancellationToken);
            return Ok(result);
        }


        [HttpGet("getAll")]
        public async Task<IActionResult> GetCountries(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? name = null,
            [FromQuery] string? slug = null,
            [FromQuery] bool? hasPicture = null,
            [FromQuery] DateTime? createdFrom = null,
            [FromQuery] DateTime? createdTo = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDir = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetAllCountriesCommand
            {
                Page = page,
                PageSize = pageSize,
                Name = name,
                Slug = slug,
                HasPicture = hasPicture,
                CreatedFrom = createdFrom,
                CreatedTo = createdTo,
                SortBy = sortBy,
                SortDir = sortDir
            }, cancellationToken);
            return Ok(result);
        }

        [HttpGet("export")]
        public async Task<IActionResult> Export([FromQuery] ExportCountriesQuery query, CancellationToken cancellationToken)
        {
            var file = await _mediator.Send(query, cancellationToken);
            return File(file.Content, file.ContentType, file.FileName);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("import")]
        [RequestSizeLimit(2 * 1024 * 1024 + 64 * 1024)]
        public async Task<IActionResult> Import([FromForm] ImportCountriesCommand request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(request, cancellationToken);
            return Ok(result);
        }
    }
}
