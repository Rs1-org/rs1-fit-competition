using Competition.Application.DTOs.PersonDto;

namespace Competition.Application.Services;

public interface IPlayerService
{
    Task<PlayerDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<PlayerDto>> GetAllAsync();
    Task<IEnumerable<PlayerDto>> SearchAsync(string term);
    Task<PlayerDto> CreateAsync(PlayerDto dto);
    Task<PlayerDto> UpdateAsync(Guid id, PlayerDto dto);
    Task DeleteAsync(Guid id);
}
