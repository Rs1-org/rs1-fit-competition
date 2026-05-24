using Competition.Application.DTOs.PersonDto;
using Competition.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers;

[Route("api/manager")]
[ApiController]
public class ManagerController : ControllerBase
{
    private readonly IManagerService _service;

    public ManagerController(IManagerService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var manager = await _service.GetByIdAsync(id);
        if (manager == null)
            throw new KeyNotFoundException($"Manager with id {id} not found");

        return Ok(manager);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpPost]
    public async Task<IActionResult> AddManager([FromBody] ManagerDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return Ok(created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateManager(Guid id, [FromBody] ManagerDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteManager(Guid id)
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
