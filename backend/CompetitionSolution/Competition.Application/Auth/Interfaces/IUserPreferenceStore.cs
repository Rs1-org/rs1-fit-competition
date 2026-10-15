namespace Competition.Application.Auth.Interfaces;

public record UserPreferenceData(string Theme, string Language, string DateFormat);

public interface IUserPreferenceStore
{
    Task<UserPreferenceData> GetAsync(Guid userId, CancellationToken cancellationToken);
    Task SaveAsync(Guid userId, UserPreferenceData preference, CancellationToken cancellationToken);
}
