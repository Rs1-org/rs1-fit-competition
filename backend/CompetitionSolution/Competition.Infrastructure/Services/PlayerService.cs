using Competition.Application.DTOs.PersonDto;
using Competition.Application.Helpers;
using Competition.Application.Services;
using Competition.Domain.Entities.Person;
using Competition.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Competition.Infrastructure.Services;

public class PlayerService : IPlayerService
{
    private readonly AppDbContext _context;

    public PlayerService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PlayerDto?> GetByIdAsync(Guid id)
    {
        var player = await ActivePlayers().FirstOrDefaultAsync(p => p.Id == id);
        return player == null ? null : MapToDto(player);
    }

    public async Task<IEnumerable<PlayerDto>> GetAllAsync()
    {
        var players = await ActivePlayers().ToListAsync();
        return players.Select(MapToDto);
    }

    public async Task<IEnumerable<PlayerDto>> SearchAsync(string term)
    {
        var players = await ActivePlayers().ToListAsync();

        return players
            .Where(p =>
                p.FirstName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                p.LastName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                p.ShirtNumber.ToString().Contains(term, StringComparison.OrdinalIgnoreCase) ||
                p.Height.ToString().Contains(term, StringComparison.OrdinalIgnoreCase))
            .Select(MapToDto);
    }

    public async Task<PlayerDto> CreateAsync(PlayerDto dto)
    {
        var player = MapToEntity(dto);
        await _context.Players.AddAsync(player);
        await _context.SaveChangesAsync();
        return MapToDto(player);
    }

    public async Task<PlayerDto> UpdateAsync(Guid id, PlayerDto dto)
    {
        var player = await ActivePlayers().FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Player with id {id} not found");

        ApplyDto(dto, player);
        await _context.SaveChangesAsync();
        return MapToDto(player);
    }

    public async Task DeleteAsync(Guid id)
    {
        var player = await ActivePlayers().FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Player with id {id} not found");

        _context.Players.Remove(player);
        await _context.SaveChangesAsync();
    }

    private IQueryable<Player> ActivePlayers() =>
        _context.Players.Where(p => p.DeletedAt == null);

    private static PlayerDto MapToDto(Player player) => new()
    {
        Id = player.Id,
        FirstName = player.FirstName,
        LastName = player.LastName,
        DateOfBirth = NodaTimeMappingHelper.FormatLocalTime(player.DateOfBirth),
        Picture = player.Picture,
        Height = player.Height,
        ShirtNumber = player.ShirtNumber,
        TransferValue = player.TransferValue
    };

    private static Player MapToEntity(PlayerDto dto) => new()
    {
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        DateOfBirth = NodaTimeMappingHelper.ParseLocalTime(dto.DateOfBirth),
        Picture = dto.Picture ?? Array.Empty<byte>(),
        Height = dto.Height,
        ShirtNumber = dto.ShirtNumber,
        TransferValue = dto.TransferValue
    };

    private static void ApplyDto(PlayerDto dto, Player player)
    {
        player.FirstName = dto.FirstName;
        player.LastName = dto.LastName;
        player.DateOfBirth = NodaTimeMappingHelper.ParseLocalTime(dto.DateOfBirth);
        player.Picture = dto.Picture ?? Array.Empty<byte>();
        player.Height = dto.Height;
        player.ShirtNumber = dto.ShirtNumber;
        player.TransferValue = dto.TransferValue;
    }
}
