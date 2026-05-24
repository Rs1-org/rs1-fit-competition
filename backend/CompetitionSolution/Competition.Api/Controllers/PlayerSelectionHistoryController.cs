using Competition.Application.DTOs.HistoryDto;
using Competition.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers;

[Route("api/player-selection-history")]
[ApiController]
public class PlayerSelectionHistoryController : ControllerBase
{
    private readonly IPlayerSelectionHistoryService _service;

    public PlayerSelectionHistoryController(IPlayerSelectionHistoryService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var record = await _service.GetByIdAsync(id);
        if (record == null)
            throw new KeyNotFoundException($"PlayerSelectionHistory with id {id} not found");

        return Ok(record);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] PlayerSelectionHistoryDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return Ok(created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] PlayerSelectionHistoryDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return Ok(id);
    }
}
