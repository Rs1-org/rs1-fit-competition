using Competition.Application.DTOs.PersonDto;
using Competition.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers;

[Route("api/referee")]
[ApiController]
public class RefereeController : ControllerBase
{
    private readonly IRefereeService _service;

    public RefereeController(IRefereeService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var referee = await _service.GetByIdAsync(id);
        if (referee == null)
            throw new KeyNotFoundException($"Referee with id {id} not found");

        return Ok(referee);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpPost]
    public async Task<IActionResult> AddReferee([FromBody] RefereeDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return Ok(created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReferee(Guid id, [FromBody] RefereeDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReferee(Guid id)
    {
        await _service.DeleteAsync(id);
        return Ok(id);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return Ok(await _service.GetAllAsync());

        return Ok(await _service.SearchAsync(term));
    }
}
