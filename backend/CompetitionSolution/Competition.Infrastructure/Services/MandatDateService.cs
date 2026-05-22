using Competition.Application.DTOs.HistoryDto;
using Competition.Application.Helpers;
using Competition.Application.Services;
using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Competition.Infrastructure.Services;

public class MandatDateService : IMandatDateService
{
    private readonly AppDbContext _context;

    public MandatDateService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MandatDateDto?> GetByIdAsync(Guid id)
    {
        var entity = await ActiveRecords().FirstOrDefaultAsync(m => m.Id == id);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<IEnumerable<MandatDateDto>> GetAllAsync()
    {
        var records = await ActiveRecords().ToListAsync();
        return records.Select(MapToDto);
    }

    public async Task<MandatDateDto> CreateAsync(MandatDateDto dto)
    {
        await ValidateForeignKeysAsync(dto);

        var entity = MapToEntity(dto);
        await _context.MandatDates.AddAsync(entity);
        await _context.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task<MandatDateDto> UpdateAsync(Guid id, MandatDateDto dto)
    {
        await ValidateForeignKeysAsync(dto);

        var entity = await ActiveRecords().FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new KeyNotFoundException($"MandatDate with id {id} not found");

        ApplyDto(dto, entity);
        await _context.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await ActiveRecords().FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new KeyNotFoundException($"MandatDate with id {id} not found");

        _context.MandatDates.Remove(entity);
        await _context.SaveChangesAsync();
    }

    private IQueryable<MandatDate> ActiveRecords() =>
        _context.MandatDates.Where(m => m.DeletedAt == null);

    private async Task ValidateForeignKeysAsync(MandatDateDto dto)
    {
        if (dto.PlayerSelectionHistoryId.HasValue)
        {
            var exists = await _context.PlayerSelectionHistories
                .AnyAsync(h => h.Id == dto.PlayerSelectionHistoryId && h.DeletedAt == null);
            if (!exists)
                throw new KeyNotFoundException($"PlayerSelectionHistory with id {dto.PlayerSelectionHistoryId} not found");
        }

        if (dto.SelectionManagerHistoryId.HasValue)
        {
            var exists = await _context.SelectionManagerHistories
                .AnyAsync(h => h.Id == dto.SelectionManagerHistoryId && h.DeletedAt == null);
            if (!exists)
                throw new KeyNotFoundException($"SelectionManagerHistory with id {dto.SelectionManagerHistoryId} not found");
        }
    }

    private static MandatDateDto MapToDto(MandatDate entity) => new()
    {
        Id = entity.Id,
        StartMandatDate = NodaTimeMappingHelper.FormatNullableLocalDate(entity.StartMandatDate),
        EndMandatDate = NodaTimeMappingHelper.FormatNullableLocalDate(entity.EndMandatDate),
        PlayerSelectionHistoryId = entity.PlayerSelectionHistoryId,
        SelectionManagerHistoryId = entity.SelectionManagerHistoryId
    };

    private static MandatDate MapToEntity(MandatDateDto dto) => new()
    {
        StartMandatDate = NodaTimeMappingHelper.ParseNullableLocalDate(dto.StartMandatDate),
        EndMandatDate = NodaTimeMappingHelper.ParseNullableLocalDate(dto.EndMandatDate),
        PlayerSelectionHistoryId = dto.PlayerSelectionHistoryId,
        SelectionManagerHistoryId = dto.SelectionManagerHistoryId
    };

    private static void ApplyDto(MandatDateDto dto, MandatDate entity)
    {
        entity.StartMandatDate = NodaTimeMappingHelper.ParseNullableLocalDate(dto.StartMandatDate);
        entity.EndMandatDate = NodaTimeMappingHelper.ParseNullableLocalDate(dto.EndMandatDate);
        entity.PlayerSelectionHistoryId = dto.PlayerSelectionHistoryId;
        entity.SelectionManagerHistoryId = dto.SelectionManagerHistoryId;
    }
}
