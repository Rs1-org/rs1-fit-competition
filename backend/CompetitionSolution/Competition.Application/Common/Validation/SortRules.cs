using FluentValidation;

namespace Competition.Application.Common.Validation;

/// <summary>Shared FluentValidation rules for <c>sortBy</c> / <c>sortDir</c> query parameters.</summary>
public static class SortRules
{
    /// <summary>Null means "default order"; otherwise the key must be one of <paramref name="allowed"/>.</summary>
    public static IRuleBuilderOptions<T, string?> MustBeSortKey<T>(this IRuleBuilder<T, string?> rule, IReadOnlyCollection<string> allowed) =>
        rule.Must(key => key is null || allowed.Contains(key, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"'{{PropertyName}}' must be one of: {string.Join(", ", allowed)}.");

    public static IRuleBuilderOptions<T, string?> MustBeSortDirection<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(dir => dir is null
                         || dir.Equals("asc", StringComparison.OrdinalIgnoreCase)
                         || dir.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("'{PropertyName}' must be 'asc' or 'desc'.");
}
