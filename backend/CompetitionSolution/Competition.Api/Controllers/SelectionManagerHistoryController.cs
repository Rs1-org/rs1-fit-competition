using Competition.Application.DTOs.HistoryDto;
using Competition.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers;

[Route("api/selection-manager-history")]
[ApiController]
public class SelectionManagerHistoryController : ControllerBase
{
    private readonly ISelectionManagerHistoryService _service;

    public SelectionManagerHistoryController(ISelectionManagerHistoryService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var record = await _service.GetByIdAsync(id);
        if (record == null)
            throw new KeyNotFoundException($"SelectionManagerHistory with id {id} not found");

        return Ok(record);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] SelectionManagerHistoryDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return Ok(created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SelectionManagerHistoryDto dto)
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
