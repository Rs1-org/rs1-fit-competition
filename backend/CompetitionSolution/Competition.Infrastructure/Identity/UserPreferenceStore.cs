using Competition.Application.Auth.Interfaces;
using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Competition.Infrastructure.Identity;

public class UserPreferenceStore(AppDbContext db) : IUserPreferenceStore
{
    public async Task<UserPreferenceData> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        var preference = await db.UserPreferences.AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        return preference is null
            ? new UserPreferenceData("light", "bs", "dd.MM.yyyy")
            : new UserPreferenceData(preference.Theme, preference.Language, preference.DateFormat);
    }

    public async Task SaveAsync(Guid userId, UserPreferenceData preference, CancellationToken cancellationToken)
    {
        var existing = await db.UserPreferences.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        if (existing is null)
        {
            db.UserPreferences.Add(new UserPreference
            {
                UserId = userId,
                Theme = preference.Theme,
                Language = preference.Language,
                DateFormat = preference.DateFormat
            });
        }
        else
        {
            existing.Theme = preference.Theme;
            existing.Language = preference.Language;
            existing.DateFormat = preference.DateFormat;
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
