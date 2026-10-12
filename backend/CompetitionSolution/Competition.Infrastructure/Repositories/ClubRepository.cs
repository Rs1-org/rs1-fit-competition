using Competition.Application.Common.Models;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Competition.Infrastructure.Repositories;

public class ClubRepository : Repository<Club>, IClubRepository
{
    private readonly AppDbContext context;
    public ClubRepository(AppDbContext context) : base(context) => this.context = context;
    private IQueryable<Club> WithDetails() => context.Clubs.Include(x => x.City).Include(x => x.Stadium);

    public Task<Club?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        WithDetails().FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);

    public Task<Club?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken = default) =>
        WithDetails().FirstOrDefaultAsync(x => x.PublicId == publicId, cancellationToken);

    public Task<PagedResult<Club>> GetPagedFilteredAsync(ClubListFilter filter, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = WithDetails().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            var name = filter.Name.Trim().ToLowerInvariant();
            query = query.Where(x => x.Name.ToLower().Contains(name));
        }
        if (filter.CityId != null) query = query.Where(x => x.City.PublicId == filter.CityId);
        if (filter.StadiumId != null) query = query.Where(x => x.Stadium.PublicId == filter.StadiumId);
        if (filter.ClubAge != null) query = query.Where(x => x.ClubAge == filter.ClubAge);
        if (filter.FoundingYear is int year)
        {
            var start = new LocalDate(year, 1, 1);
            var end = new LocalDate(year, 12, 31);
            query = query.Where(x => x.FoundingDate >= start && x.FoundingDate <= end);
        }
        return query.OrderBy(x => x.Name).ThenBy(x => x.Id).ToPagedResultAsync(page, pageSize, cancellationToken);
    }
}
