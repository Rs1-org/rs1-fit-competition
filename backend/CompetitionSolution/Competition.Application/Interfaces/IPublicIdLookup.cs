using Competition.Domain.Entities;

namespace Competition.Application.Interfaces;

/// <summary>
/// Translates the PublicId the frontend sends into the internal Id (RS1 item 135).
/// </summary>
public interface IPublicIdLookup
{
    /// <summary>Returns the internal Id, or null when no active entity has this PublicId.</summary>
    Task<Guid?> GetIdByPublicIdAsync<TEntity>(string? publicId, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity;

    /// <summary>Returns the internal Id or throws <see cref="KeyNotFoundException"/> (mapped to 404).</summary>
    Task<Guid> GetRequiredIdAsync<TEntity>(string? publicId, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity;
}
