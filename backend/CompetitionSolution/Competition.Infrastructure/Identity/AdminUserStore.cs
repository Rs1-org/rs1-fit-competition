using Competition.Application.Admin.Users;
using Competition.Application.Common.Models;
using Competition.Application.Helpers;
using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Text;

namespace Competition.Infrastructure.Identity;

public class AdminUserStore(AppDbContext db, IClock clock) : IAdminUserStore
{
    public async Task<PagedResult<UserListDto>> GetFilteredAsync(UserFilter filter, Guid currentUserId, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLowerInvariant();
            query = query.Where(x => (x.Email != null && x.Email.ToLower().Contains(search))
                || x.FirstName.ToLower().Contains(search) || x.LastName.ToLower().Contains(search)
                || (x.FirstName + " " + x.LastName).ToLower().Contains(search));
        }
        if (filter.Role is not null)
            query = query.Where(x => db.UserRoles.Any(ur => ur.UserId == x.Id
                && db.Roles.Any(r => r.Id == ur.RoleId && r.Name == filter.Role)));
        if (filter.IsActive.HasValue)
            query = query.Where(x => x.IsActive == filter.IsActive.Value);
        if (filter.RegisteredFrom.HasValue)
            query = query.Where(x => x.RegisteredAt >= filter.RegisteredFrom.Value);
        if (filter.RegisteredTo.HasValue)
            query = query.Where(x => x.RegisteredAt <= filter.RegisteredTo.Value);
        var result = await query.OrderBy(x => x.NormalizedEmail).ThenBy(x => x.PublicId).ToPagedResultAsync(page, pageSize, ct);
        var ids = result.Items.Select(x => x.Id).ToList();
        var roles = await (from ur in db.UserRoles.AsNoTracking()
            join role in db.Roles.AsNoTracking() on ur.RoleId equals role.Id
            where ids.Contains(ur.UserId)
            select new { ur.UserId, role.Name }).ToListAsync(ct);
        var items = result.Items.Select(x => new UserListDto(x.PublicId, x.Email ?? "", x.FirstName, x.LastName,
            x.IsActive, x.RegisteredAt.HasValue ? InstantPattern.ExtendedIso.Format(x.RegisteredAt.Value) : null,
            roles.Where(r => r.UserId == x.Id).Select(r => r.Name!).Order().ToList(), x.Id == currentUserId)).ToList();
        return new PagedResult<UserListDto>(items, result.TotalCount, result.Page, result.PageSize);
    }

    public Task<ApplicationUser?> FindAsync(string publicId, CancellationToken ct) =>
        db.Users.FirstOrDefaultAsync(x => x.PublicId == publicId, ct);

    public async Task SetRoleAsync(ApplicationUser user, string role, CancellationToken ct)
    {
        var roleId = await db.Roles.Where(x => x.Name == role).Select(x => (Guid?)x.Id).SingleOrDefaultAsync(ct)
            ?? throw BusinessRule.Violation("Role", "The selected role is not configured.");
        var memberships = await db.UserRoles.Where(x => x.UserId == user.Id).ToListAsync(ct);
        if (memberships.Count == 1 && memberships[0].RoleId == roleId) return;
        db.UserRoles.RemoveRange(memberships.Where(x => x.RoleId != roleId));
        if (memberships.All(x => x.RoleId != roleId))
            db.UserRoles.Add(new IdentityUserRole<Guid> { UserId = user.Id, RoleId = roleId });
        await RevokeSessionsAsync(user, ct);
    }

    public async Task SetActiveAsync(ApplicationUser user, bool active, CancellationToken ct)
    {
        if (user.IsActive == active) return;
        user.IsActive = active;
        await RevokeSessionsAsync(user, ct);
    }

    private async Task RevokeSessionsAsync(ApplicationUser user, CancellationToken ct)
    {
        user.SecurityStamp = Guid.NewGuid().ToString("N");
        var tokens = await db.RefreshTokens.Where(x => x.UserId == user.Id && !x.IsRevoked).ToListAsync(ct);
        foreach (var token in tokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = clock.GetCurrentInstant().ToDateTimeUtc();
        }
    }
}
