using Competition.Application.DTOs.PersonDto;
using Competition.Application.Helpers;
using Competition.Application.Services;
using Competition.Domain.Entities.Person;
using Competition.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Competition.Infrastructure.Services;

public class ManagerService : IManagerService
{
    private readonly AppDbContext _context;

    public ManagerService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ManagerDto?> GetByIdAsync(Guid id)
    {
        var manager = await ActiveManagers().FirstOrDefaultAsync(m => m.Id == id);
        return manager == null ? null : MapToDto(manager);
    }

    public async Task<IEnumerable<ManagerDto>> GetAllAsync()
    {
        var managers = await ActiveManagers().ToListAsync();
        return managers.Select(MapToDto);
    }

    public async Task<IEnumerable<ManagerDto>> SearchAsync(string term)
    {
        var managers = await ActiveManagers().ToListAsync();

        return managers
            .Where(m =>
                m.FirstName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                m.LastName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (m.CareerStart != null &&
                 NodaTimeMappingHelper.FormatNullableLocalTime(m.CareerStart)!.Contains(term, StringComparison.OrdinalIgnoreCase)))
            .Select(MapToDto);
    }

    public async Task<ManagerDto> CreateAsync(ManagerDto dto)
    {
        var manager = MapToEntity(dto);
        await _context.Managers.AddAsync(manager);
        await _context.SaveChangesAsync();
        return MapToDto(manager);
    }

    public async Task<ManagerDto> UpdateAsync(Guid id, ManagerDto dto)
    {
        var manager = await ActiveManagers().FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new KeyNotFoundException($"Manager with id {id} not found");

        ApplyDto(dto, manager);
        await _context.SaveChangesAsync();
        return MapToDto(manager);
    }

    public async Task DeleteAsync(Guid id)
    {
        var manager = await ActiveManagers().FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new KeyNotFoundException($"Manager with id {id} not found");

        _context.Managers.Remove(manager);
        await _context.SaveChangesAsync();
    }

    private IQueryable<Manager> ActiveManagers() =>
        _context.Managers.Where(m => m.DeletedAt == null);

    private static ManagerDto MapToDto(Manager manager) => new()
    {
        Id = manager.Id,
        FirstName = manager.FirstName,
        LastName = manager.LastName,
        DateOfBirth = NodaTimeMappingHelper.FormatLocalTime(manager.DateOfBirth),
        Picture = manager.Picture,
        CareerStart = NodaTimeMappingHelper.FormatNullableLocalTime(manager.CareerStart)
    };

    private static Manager MapToEntity(ManagerDto dto) => new()
    {
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        DateOfBirth = NodaTimeMappingHelper.ParseLocalTime(dto.DateOfBirth),
        Picture = dto.Picture ?? Array.Empty<byte>(),
        CareerStart = NodaTimeMappingHelper.ParseNullableLocalTime(dto.CareerStart)
    };

    private static void ApplyDto(ManagerDto dto, Manager manager)
    {
        manager.FirstName = dto.FirstName;
        manager.LastName = dto.LastName;
        manager.DateOfBirth = NodaTimeMappingHelper.ParseLocalTime(dto.DateOfBirth);
        manager.Picture = dto.Picture ?? Array.Empty<byte>();
        manager.CareerStart = NodaTimeMappingHelper.ParseNullableLocalTime(dto.CareerStart);
    }
}
