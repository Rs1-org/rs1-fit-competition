using Competition.Application.Common.Models;
using Competition.Domain.Entities;
using NodaTime;

namespace Competition.Application.Interfaces;

public record ClubListFilter(string? Name, Guid? CountryId, Guid? CityId,
    LocalDate? FoundedFrom, LocalDate? FoundedTo, bool? HasStadium, ClubAge? ClubAge,
    string SortBy = "name", bool Descending = false);

public interface IClub : IRepository<Club>
{
    Task<Club?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<PagedResult<Club>> GetPagedFilteredAsync(ClubListFilter filter, int page, int pageSize,
        CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId, CancellationToken cancellationToken = default);
}
