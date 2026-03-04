namespace Competition.Application.Helpers;

public static class SlugHelper
{
    public static string GenerateSlug(string text)
    {
        return text
            .ToLower()
            .Replace(" ", "-")
            .Replace("_", "-");
    }
}