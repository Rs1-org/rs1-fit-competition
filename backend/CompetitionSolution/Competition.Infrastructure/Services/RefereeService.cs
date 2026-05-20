using Competition.Application.DTOs.PersonDto;
using Competition.Application.Helpers;
using Competition.Application.Services;
using Competition.Domain.Entities.Person;
using Competition.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Competition.Infrastructure.Services;

public class RefereeService : IRefereeService
{
    private readonly AppDbContext _context;

    public RefereeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RefereeDto?> GetByIdAsync(Guid id)
    {
        var referee = await ActiveReferees().FirstOrDefaultAsync(r => r.Id == id);
        return referee == null ? null : MapToDto(referee);
    }

    public async Task<IEnumerable<RefereeDto>> GetAllAsync()
    {
        var referees = await ActiveReferees().ToListAsync();
        return referees.Select(MapToDto);
    }

    public async Task<IEnumerable<RefereeDto>> SearchAsync(string term)
    {
        var referees = await ActiveReferees().ToListAsync();

        return referees
            .Where(r =>
                r.FirstName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                r.LastName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (r.CareerStart != null &&
                 NodaTimeMappingHelper.FormatNullableLocalDate(r.CareerStart)!.Contains(term, StringComparison.OrdinalIgnoreCase)))
            .Select(MapToDto);
    }

    public async Task<RefereeDto> CreateAsync(RefereeDto dto)
    {
        var referee = MapToEntity(dto);
        await _context.Referees.AddAsync(referee);
        await _context.SaveChangesAsync();
        return MapToDto(referee);
    }

    public async Task<RefereeDto> UpdateAsync(Guid id, RefereeDto dto)
    {
        var referee = await ActiveReferees().FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new KeyNotFoundException($"Referee with id {id} not found");

        ApplyDto(dto, referee);
        await _context.SaveChangesAsync();
        return MapToDto(referee);
    }

    public async Task DeleteAsync(Guid id)
    {
        var referee = await ActiveReferees().FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new KeyNotFoundException($"Referee with id {id} not found");

        _context.Referees.Remove(referee);
        await _context.SaveChangesAsync();
    }

    private IQueryable<Referee> ActiveReferees() =>
        _context.Referees.Where(r => r.DeletedAt == null);

    private static RefereeDto MapToDto(Referee referee) => new()
    {
        Id = referee.Id,
        FirstName = referee.FirstName,
        LastName = referee.LastName,
        DateOfBirth = NodaTimeMappingHelper.FormatLocalDate(referee.DateOfBirth),
        Picture = referee.Picture,
        CareerStart = NodaTimeMappingHelper.FormatNullableLocalDate(referee.CareerStart)
    };

    private static Referee MapToEntity(RefereeDto dto) => new()
    {
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        DateOfBirth = NodaTimeMappingHelper.ParseLocalDate(dto.DateOfBirth),
        Picture = dto.Picture ?? Array.Empty<byte>(),
        CareerStart = NodaTimeMappingHelper.ParseNullableLocalDate(dto.CareerStart)
    };

    private static void ApplyDto(RefereeDto dto, Referee referee)
    {
        referee.FirstName = dto.FirstName;
        referee.LastName = dto.LastName;
        referee.DateOfBirth = NodaTimeMappingHelper.ParseLocalDate(dto.DateOfBirth);
        if (dto.Picture != null)
            referee.Picture = dto.Picture;
        referee.CareerStart = NodaTimeMappingHelper.ParseNullableLocalDate(dto.CareerStart);
    }
}
