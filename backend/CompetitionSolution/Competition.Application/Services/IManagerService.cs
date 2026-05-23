using Competition.Application.DTOs.PersonDto;

namespace Competition.Application.Services;

public interface IManagerService
{
    Task<ManagerDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ManagerDto>> GetAllAsync();
    Task<IEnumerable<ManagerDto>> SearchAsync(string term);
    Task<ManagerDto> CreateAsync(ManagerDto dto);
    Task<ManagerDto> UpdateAsync(Guid id, ManagerDto dto);
    Task DeleteAsync(Guid id);
}
