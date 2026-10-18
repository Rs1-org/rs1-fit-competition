using Microsoft.EntityFrameworkCore;

namespace Competition.Infrastructure.Data;

public static class SampleAccountProtection
{
    // Fingerprints of the published sample hashes in SeedTestAdminAndModerator.
    // A rotated password or a different account is deliberately left alone.
    public static async Task DisableDefaultPasswordsAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        var adminId = Guid.Parse("b1111111-1111-1111-1111-111111111111");
        var moderatorId = Guid.Parse("b2222222-2222-2222-2222-222222222222");
        var users = await db.Users.Where(u =>
            (u.Id == adminId && u.PasswordHash == "AQAAAAIAAYagAAAAEFMV98tOqinb6+aZ4euyZT9LY/p40bx8Tu1qC7qWU0N5NhPYn51B28pZbkn/Yj6gFw==") ||
            (u.Id == moderatorId && u.PasswordHash == "AQAAAAIAAYagAAAAEKOASaeH5BjFk28mJA/3/E894drLqlvrnbxckWXtOIe6iXLbRH+LiYSSnlPMWwzOUQ=="))
            .ToListAsync(cancellationToken);

        if (users.Count == 0) return;
        var userIds = users.Select(u => u.Id).ToArray();
        foreach (var user in users)
        {
            user.PasswordHash = null;
            user.SecurityStamp = Guid.NewGuid().ToString();
        }
        db.RefreshTokens.RemoveRange(await db.RefreshTokens
            .Where(t => userIds.Contains(t.UserId)).ToListAsync(cancellationToken));
        await db.SaveChangesAsync(cancellationToken);
    }
}
