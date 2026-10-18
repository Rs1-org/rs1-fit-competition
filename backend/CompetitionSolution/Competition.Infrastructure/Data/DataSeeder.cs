using Competition.Domain.Entities;
using Competition.Infrastructure.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Competition.Domain.Entities.MatchEntities;

namespace Competition.Infrastructure.Data;

/// <summary>Startup seed entry point. Domain seed methods can be added here in FK order.</summary>
public class DataSeeder(
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager,
    IOptions<SeedSettings> options,
    AppDbContext db)
{
    public static IReadOnlyList<string> Roles { get; } = Array.AsReadOnly(new[] { "Admin", "Moderator", "User", "Manager" });

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedRolesAsync(cancellationToken);
        await SeedEventLookupsAsync(db, cancellationToken);
        if (options.Value.Enabled)
            await SeedUsersAsync(cancellationToken);
    }

    /// <summary>Required reference data, independent of the optional demo-user switch.</summary>
    public static async Task SeedEventLookupsAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        (string Code, string Name)[] types =
        [
            ("goal", "Goal"), ("own-goal", "Own goal"), ("yellow-card", "Yellow card"),
            ("red-card", "Red card"), ("substitution", "Substitution"),
            ("penalty", "Penalty scored"), ("penalty-missed", "Penalty missed"),
        ];
        (string Code, string Name)[] parts =
        [
            ("first-half", "1st half"), ("second-half", "2nd half"),
            ("extra-time", "Extra time"), ("penalties", "Penalties"),
        ];
        var existingTypes = await context.EventTypes.Select(x => x.Code).ToListAsync(cancellationToken);
        var existingParts = await context.MatchParts.Select(x => x.Code).ToListAsync(cancellationToken);
        for (var i = 0; i < types.Length; i++)
            if (!existingTypes.Contains(types[i].Code))
                context.EventTypes.Add(new EventType { Code = types[i].Code, Name = types[i].Name, SortOrder = i });
        for (var i = 0; i < parts.Length; i++)
            if (!existingParts.Contains(parts[i].Code))
                context.MatchParts.Add(new MatchPart { Code = parts[i].Code, Name = parts[i].Name, SortOrder = i });
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedRolesAsync(CancellationToken cancellationToken)
    {
        foreach (var role in Roles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!await roleManager.RoleExistsAsync(role))
                EnsureSucceeded(await roleManager.CreateAsync(new ApplicationRole { Name = role }));
        }
    }

    private async Task SeedUsersAsync(CancellationToken cancellationToken)
    {
        var settings = options.Value;
        (string Role, string Password)[] accounts =
        [
            ("Admin", settings.AdminPassword), ("Moderator", settings.ModeratorPassword),
            ("User", settings.UserPassword), ("Manager", settings.ManagerPassword),
        ];

        // Validate every configured password before creating any account. Never log password values.
        foreach (var (role, password) in accounts)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException($"Seed:{role}Password is required when Seed:Enabled is true.");
            var candidate = NewUser(role);
            foreach (var validator in userManager.PasswordValidators)
                EnsureSucceeded(await validator.ValidateAsync(userManager, candidate, password));
        }

        foreach (var (role, password) in accounts)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var candidate = NewUser(role);
            var existing = await userManager.FindByEmailAsync(candidate.Email!);
            if (existing is not null)
            {
                // Do not reset passwords, reactivate users or grant privileges to existing accounts.
                if (!await userManager.IsInRoleAsync(existing, role))
                    throw new InvalidOperationException($"The {role} seed email is already used by an account without that role. Resolve it manually.");
                continue;
            }

            EnsureSucceeded(await userManager.CreateAsync(candidate, password));
            var assignment = await userManager.AddToRoleAsync(candidate, role);
            if (!assignment.Succeeded)
            {
                // Allow a later startup to retry after an unsuccessful role assignment.
                EnsureSucceeded(await userManager.DeleteAsync(candidate));
                EnsureSucceeded(assignment);
            }
        }
    }

    private static ApplicationUser NewUser(string role)
    {
        var email = $"{role.ToLowerInvariant()}@competition.ba";
        return new ApplicationUser
        {
            UserName = email, Email = email, EmailConfirmed = true,
            FirstName = "Demo", LastName = role,
        };
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException($"Identity seeding failed: {string.Join(", ", result.Errors.Select(e => e.Code))}.");
    }
}
