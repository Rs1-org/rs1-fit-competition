using Competition.Application.DTOs.HistoryDto;
using Competition.Application.Services;
using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Competition.Infrastructure.Services;

public class PlayerSelectionHistoryService : IPlayerSelectionHistoryService
{
    private readonly AppDbContext _context;

    public PlayerSelectionHistoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PlayerSelectionHistoryDto?> GetByIdAsync(Guid id)
    {
        var entity = await ActiveRecords().FirstOrDefaultAsync(h => h.Id == id);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<IEnumerable<PlayerSelectionHistoryDto>> GetAllAsync()
    {
        var records = await ActiveRecords().ToListAsync();
        return records.Select(MapToDto);
    }

    public async Task<PlayerSelectionHistoryDto> CreateAsync(PlayerSelectionHistoryDto dto)
    {
        await EnsurePlayerExistsAsync(dto.PlayerId);
        await EnsureSelectionExistsAsync(dto.SelectionId);

        var entity = MapToEntity(dto);
        await _context.PlayerSelectionHistories.AddAsync(entity);
        await _context.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task<PlayerSelectionHistoryDto> UpdateAsync(Guid id, PlayerSelectionHistoryDto dto)
    {
        await EnsurePlayerExistsAsync(dto.PlayerId);
        await EnsureSelectionExistsAsync(dto.SelectionId);

        var entity = await ActiveRecords().FirstOrDefaultAsync(h => h.Id == id)
            ?? throw new KeyNotFoundException($"PlayerSelectionHistory with id {id} not found");

        ApplyDto(dto, entity);
        await _context.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await ActiveRecords().FirstOrDefaultAsync(h => h.Id == id)
            ?? throw new KeyNotFoundException($"PlayerSelectionHistory with id {id} not found");

        _context.PlayerSelectionHistories.Remove(entity);
        await _context.SaveChangesAsync();
    }

    private IQueryable<PlayerSelectionHistory> ActiveRecords() =>
        _context.PlayerSelectionHistories.Where(h => h.DeletedAt == null);

    private async Task EnsurePlayerExistsAsync(Guid playerId)
    {
        var exists = await _context.Players.AnyAsync(p => p.Id == playerId && p.DeletedAt == null);
        if (!exists)
            throw new KeyNotFoundException($"Player with id {playerId} not found");
    }

    private async Task EnsureSelectionExistsAsync(Guid? selectionId)
    {
        if (selectionId is null) return;
        var exists = await _context.Selections.AnyAsync(s => s.Id == selectionId && s.DeletedAt == null);
        if (!exists)
            throw new KeyNotFoundException($"Selection with id {selectionId} not found");
    }

    private static PlayerSelectionHistoryDto MapToDto(PlayerSelectionHistory entity) => new()
    {
        Id = entity.Id,
        Goals = entity.Goals,
        MatchPlayed = entity.MatchPlayed,
        Assists = entity.Assists,
        ShirtNumber = entity.ShirtNumber,
        PlayerId = entity.PlayerId,
        SelectionId = entity.SelectionId
    };

    private static PlayerSelectionHistory MapToEntity(PlayerSelectionHistoryDto dto) => new()
    {
        Goals = dto.Goals,
        MatchPlayed = dto.MatchPlayed,
        Assists = dto.Assists,
        ShirtNumber = dto.ShirtNumber,
        PlayerId = dto.PlayerId,
        SelectionId = dto.SelectionId
    };

    private static void ApplyDto(PlayerSelectionHistoryDto dto, PlayerSelectionHistory entity)
    {
        entity.Goals = dto.Goals;
        entity.MatchPlayed = dto.MatchPlayed;
        entity.Assists = dto.Assists;
        entity.ShirtNumber = dto.ShirtNumber;
        entity.PlayerId = dto.PlayerId;
        entity.SelectionId = dto.SelectionId;
    }
}
