namespace Competition.Application.Helpers;

public static class PersonValidationHelper
{
    public static bool BeValidDate(string? value) =>
        !string.IsNullOrWhiteSpace(value) && DateOnly.TryParse(value, out _);

    public static bool BeValidOptionalDate(string? value) =>
        string.IsNullOrWhiteSpace(value) || DateOnly.TryParse(value, out _);
}
