using Competition.Application.DTOs.PersonDto;
using Competition.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers;

[Route("api/player")]
[ApiController]
public class PlayerController : ControllerBase
{
    private readonly IPlayerService _service;

    public PlayerController(IPlayerService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var player = await _service.GetByIdAsync(id);
        if (player == null)
            throw new KeyNotFoundException($"Player with id {id} not found");

        return Ok(player);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpPost]
    public async Task<IActionResult> AddPlayer([FromBody] PlayerDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return Ok(created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePlayer(Guid id, [FromBody] PlayerDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlayer(Guid id)
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
