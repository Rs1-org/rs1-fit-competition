using Competition.Application.DTOs.HistoryDto;

namespace Competition.Application.Services;

public interface ISelectionManagerHistoryService
{
    Task<SelectionManagerHistoryDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<SelectionManagerHistoryDto>> GetAllAsync();
    Task<SelectionManagerHistoryDto> CreateAsync(SelectionManagerHistoryDto dto);
    Task<SelectionManagerHistoryDto> UpdateAsync(Guid id, SelectionManagerHistoryDto dto);
    Task DeleteAsync(Guid id);
}
