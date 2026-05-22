using Competition.Application.DTOs.HistoryDto;

namespace Competition.Application.Services;

public interface ITransferValueHistoryService
{
    Task<TransferValueHistoryDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<TransferValueHistoryDto>> GetAllAsync();
    Task<TransferValueHistoryDto> CreateAsync(TransferValueHistoryDto dto);
    Task<TransferValueHistoryDto> UpdateAsync(Guid id, TransferValueHistoryDto dto);
    Task DeleteAsync(Guid id);
}
