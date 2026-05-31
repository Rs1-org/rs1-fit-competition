using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Competition.Application.Helpers;

public static class SlugHelper
{
    public static string GenerateSlug(string text)
    {
        var lower = text.Trim().ToLowerInvariant();

        // Explicit map for characters that don't decompose via NFD (e.g. Đ → d)
        lower = lower.Replace("đ", "d").Replace("ð", "d");

        // Strip diacritics via Unicode normalization
        var normalized = lower.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        var slug = sb.ToString().Normalize(NormalizationForm.FormC);
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");  // remove non-alphanumeric
        slug = Regex.Replace(slug, @"\s+", "-");             // whitespace → dash
        slug = Regex.Replace(slug, @"-{2,}", "-");           // collapse multiple dashes
        return slug.Trim('-');
    }
}
