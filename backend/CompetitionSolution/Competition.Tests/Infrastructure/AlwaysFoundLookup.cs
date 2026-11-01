using Competition.Application.Interfaces;
using Competition.Domain.Entities;

namespace Competition.Tests.Infrastructure;

public sealed class AlwaysFoundLookup : IPublicIdLookup
{
    public Task<Guid?> GetIdByPublicIdAsync<TEntity>(string? publicId, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity =>
        Task.FromResult<Guid?>(Guid.NewGuid());

    public Task<Guid> GetRequiredIdAsync<TEntity>(string? publicId, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity =>
        Task.FromResult(Guid.NewGuid());

    public Task<Guid?> GetOptionalIdAsync<TEntity>(string? publicId, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity =>
        Task.FromResult<Guid?>(string.IsNullOrWhiteSpace(publicId) ? null : Guid.NewGuid());

    public Task<TEntity> GetRequiredEntityAsync<TEntity>(string? publicId, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity =>
        throw new NotSupportedException();
}
