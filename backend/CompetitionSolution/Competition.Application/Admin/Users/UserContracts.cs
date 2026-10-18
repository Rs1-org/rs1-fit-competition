using Competition.Application.Common.Models;
using Competition.Domain.Entities;
using NodaTime;

namespace Competition.Application.Admin.Users;

public record UserListDto(string PublicId, string Email, string FirstName, string LastName,
    bool IsActive, string? RegisteredAt, IReadOnlyList<string> Roles, bool IsSelf);
public record UserFilter(string? Search, string? Role, bool? IsActive, Instant? RegisteredFrom, Instant? RegisteredTo);

public interface IAdminUserStore
{
    Task<PagedResult<UserListDto>> GetFilteredAsync(UserFilter filter, Guid currentUserId, int page, int pageSize, CancellationToken ct);
    Task<ApplicationUser?> FindAsync(string publicId, CancellationToken ct);
    Task SetRoleAsync(ApplicationUser user, string role, CancellationToken ct);
    Task SetActiveAsync(ApplicationUser user, bool active, CancellationToken ct);
}
