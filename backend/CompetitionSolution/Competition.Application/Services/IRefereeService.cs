using Competition.Application.DTOs.PersonDto;

namespace Competition.Application.Services;

public interface IRefereeService
{
    Task<RefereeDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<RefereeDto>> GetAllAsync();
    Task<IEnumerable<RefereeDto>> SearchAsync(string term);
    Task<RefereeDto> CreateAsync(RefereeDto dto);
    Task<RefereeDto> UpdateAsync(Guid id, RefereeDto dto);
    Task DeleteAsync(Guid id);
}
