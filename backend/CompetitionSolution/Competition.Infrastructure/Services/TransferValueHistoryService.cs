using Competition.Application.DTOs.HistoryDto;
using Competition.Application.Helpers;
using Competition.Application.Services;
using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Competition.Infrastructure.Services;

public class TransferValueHistoryService : ITransferValueHistoryService
{
    private readonly AppDbContext _context;

    public TransferValueHistoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TransferValueHistoryDto?> GetByIdAsync(Guid id)
    {
        var entity = await ActiveRecords().FirstOrDefaultAsync(h => h.Id == id);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<IEnumerable<TransferValueHistoryDto>> GetAllAsync()
    {
        var records = await ActiveRecords().ToListAsync();
        return records.Select(MapToDto);
    }

    public async Task<TransferValueHistoryDto> CreateAsync(TransferValueHistoryDto dto)
    {
        await EnsurePlayerExistsAsync(dto.PlayerId);

        var entity = MapToEntity(dto);
        await _context.TransferValueHistories.AddAsync(entity);
        await _context.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task<TransferValueHistoryDto> UpdateAsync(Guid id, TransferValueHistoryDto dto)
    {
        await EnsurePlayerExistsAsync(dto.PlayerId);

        var entity = await ActiveRecords().FirstOrDefaultAsync(h => h.Id == id)
            ?? throw new KeyNotFoundException($"TransferValueHistory with id {id} not found");

        ApplyDto(dto, entity);
        await _context.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await ActiveRecords().FirstOrDefaultAsync(h => h.Id == id)
            ?? throw new KeyNotFoundException($"TransferValueHistory with id {id} not found");

        _context.TransferValueHistories.Remove(entity);
        await _context.SaveChangesAsync();
    }

    private IQueryable<TransferValueHistory> ActiveRecords() =>
        _context.TransferValueHistories.Where(h => h.DeletedAt == null);

    private async Task EnsurePlayerExistsAsync(Guid playerId)
    {
        var exists = await _context.Players.AnyAsync(p => p.Id == playerId && p.DeletedAt == null);
        if (!exists)
            throw new KeyNotFoundException($"Player with id {playerId} not found");
    }

    private static TransferValueHistoryDto MapToDto(TransferValueHistory entity) => new()
    {
        Id = entity.Id,
        Date = NodaTimeMappingHelper.FormatNullableLocalDate(entity.Date),
        Value = entity.Value,
        PlayerId = entity.PlayerId
    };

    private static TransferValueHistory MapToEntity(TransferValueHistoryDto dto) => new()
    {
        Date = NodaTimeMappingHelper.ParseNullableLocalDate(dto.Date),
        Value = dto.Value,
        PlayerId = dto.PlayerId
    };

    private static void ApplyDto(TransferValueHistoryDto dto, TransferValueHistory entity)
    {
        entity.Date = NodaTimeMappingHelper.ParseNullableLocalDate(dto.Date);
        entity.Value = dto.Value;
        entity.PlayerId = dto.PlayerId;
    }
}
