using Competition.Application.Common.Models;
using Competition.Domain.Entities;

namespace Competition.Application.Interfaces;

public interface IClubRepository : IRepository<Club>
{
    Task<Club?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<Club?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken = default);
    Task<PagedResult<Club>> GetPagedFilteredAsync(ClubListFilter filter, int page, int pageSize, CancellationToken cancellationToken = default);
}
