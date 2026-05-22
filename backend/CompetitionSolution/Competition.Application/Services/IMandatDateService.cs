using Competition.Application.DTOs.HistoryDto;

namespace Competition.Application.Services;

public interface IMandatDateService
{
    Task<MandatDateDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<MandatDateDto>> GetAllAsync();
    Task<MandatDateDto> CreateAsync(MandatDateDto dto);
    Task<MandatDateDto> UpdateAsync(Guid id, MandatDateDto dto);
    Task DeleteAsync(Guid id);
}
