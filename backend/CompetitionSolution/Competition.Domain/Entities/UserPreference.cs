namespace Competition.Domain.Entities;

public class UserPreference
{
    public Guid UserId { get; set; }
    public string Theme { get; set; } = "light";
    public string Language { get; set; } = "bs";
    public string DateFormat { get; set; } = "dd.MM.yyyy";
    public ApplicationUser? User { get; set; }
}
