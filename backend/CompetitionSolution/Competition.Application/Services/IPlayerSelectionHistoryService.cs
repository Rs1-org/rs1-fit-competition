using Competition.Application.DTOs.HistoryDto;

namespace Competition.Application.Services;

public interface IPlayerSelectionHistoryService
{
    Task<PlayerSelectionHistoryDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<PlayerSelectionHistoryDto>> GetAllAsync();
    Task<PlayerSelectionHistoryDto> CreateAsync(PlayerSelectionHistoryDto dto);
    Task<PlayerSelectionHistoryDto> UpdateAsync(Guid id, PlayerSelectionHistoryDto dto);
    Task DeleteAsync(Guid id);
}
