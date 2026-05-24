using Competition.Application.DTOs.HistoryDto;
using Competition.Application.Services;
using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Competition.Infrastructure.Services;

public class SelectionManagerHistoryService : ISelectionManagerHistoryService
{
    private readonly AppDbContext _context;

    public SelectionManagerHistoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SelectionManagerHistoryDto?> GetByIdAsync(Guid id)
    {
        var entity = await ActiveRecords().FirstOrDefaultAsync(h => h.Id == id);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<IEnumerable<SelectionManagerHistoryDto>> GetAllAsync()
    {
        var records = await ActiveRecords().ToListAsync();
        return records.Select(MapToDto);
    }

    public async Task<SelectionManagerHistoryDto> CreateAsync(SelectionManagerHistoryDto dto)
    {
        await EnsureManagerExistsAsync(dto.ManagerId);

        var entity = MapToEntity(dto);
        await _context.SelectionManagerHistories.AddAsync(entity);
        await _context.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task<SelectionManagerHistoryDto> UpdateAsync(Guid id, SelectionManagerHistoryDto dto)
    {
        await EnsureManagerExistsAsync(dto.ManagerId);

        var entity = await ActiveRecords().FirstOrDefaultAsync(h => h.Id == id)
            ?? throw new KeyNotFoundException($"SelectionManagerHistory with id {id} not found");

        ApplyDto(dto, entity);
        await _context.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await ActiveRecords().FirstOrDefaultAsync(h => h.Id == id)
            ?? throw new KeyNotFoundException($"SelectionManagerHistory with id {id} not found");

        _context.SelectionManagerHistories.Remove(entity);
        await _context.SaveChangesAsync();
    }

    private IQueryable<SelectionManagerHistory> ActiveRecords() =>
        _context.SelectionManagerHistories.Where(h => h.DeletedAt == null);

    private async Task EnsureManagerExistsAsync(Guid managerId)
    {
        var exists = await _context.Managers.AnyAsync(m => m.Id == managerId && m.DeletedAt == null);
        if (!exists)
            throw new KeyNotFoundException($"Manager with id {managerId} not found");
    }

    private static SelectionManagerHistoryDto MapToDto(SelectionManagerHistory entity) => new()
    {
        Id = entity.Id,
        Notes = entity.Notes,
        Wins = entity.Wins,
        Loses = entity.Loses,
        Draw = entity.Draw,
        ManagerId = entity.ManagerId
    };

    private static SelectionManagerHistory MapToEntity(SelectionManagerHistoryDto dto) => new()
    {
        Notes = dto.Notes,
        Wins = dto.Wins,
        Loses = dto.Loses,
        Draw = dto.Draw,
        ManagerId = dto.ManagerId
    };

    private static void ApplyDto(SelectionManagerHistoryDto dto, SelectionManagerHistory entity)
    {
        entity.Notes = dto.Notes;
        entity.Wins = dto.Wins;
        entity.Loses = dto.Loses;
        entity.Draw = dto.Draw;
        entity.ManagerId = dto.ManagerId;
    }
}
