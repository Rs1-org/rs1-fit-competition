using System.Text.RegularExpressions;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using FluentValidation;

namespace Competition.Application.Common.Validation;

/// <summary>
/// Shared FluentValidation rules for PublicId fields (RS1 item 135).
/// </summary>
public static partial class PublicIdRules
{
    [GeneratedRegex("^[0-9a-f]{32}$")]
    private static partial Regex PublicIdFormat();

    public static bool IsValidFormat(string? publicId) =>
        publicId is not null && PublicIdFormat().IsMatch(publicId);

    /// <summary>Value must look like a PublicId (32 lowercase hex characters).</summary>
    public static IRuleBuilderOptions<T, string?> MustBeValidPublicId<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(IsValidFormat)
            .WithMessage("'{PropertyName}' is not a valid public id.");

    /// <summary>
    /// Value must be the PublicId of an existing (not deleted) <typeparamref name="TEntity"/>.
    /// Unknown ids fail validation (400) instead of reaching the database as a broken FK.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> MustReferenceExisting<T, TEntity>(
        this IRuleBuilder<T, string?> rule, IPublicIdLookup lookup)
        where TEntity : BaseEntity =>
        rule.MustBeValidPublicId()
            .MustAsync(async (publicId, ct) => await lookup.GetIdByPublicIdAsync<TEntity>(publicId, ct) is not null)
            .WithMessage($"{typeof(TEntity).Name} with this public id does not exist.");
}
