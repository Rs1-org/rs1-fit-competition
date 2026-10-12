using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Competition.Infrastructure.Repositories;

public class PublicIdLookup : IPublicIdLookup
{
    private readonly AppDbContext _context;

    public PublicIdLookup(AppDbContext context)
        => _context = context;

    public async Task<Guid?> GetIdByPublicIdAsync<TEntity>(string? publicId, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
    {
        if (string.IsNullOrWhiteSpace(publicId))
            return null;

        // Explicit DeletedAt check: not every entity has a soft-delete query filter (e.g. Country, City, Stadium).
        return await _context.Set<TEntity>()
            .Where(e => e.PublicId == publicId && e.DeletedAt == null)
            .Select(e => (Guid?)e.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Guid> GetRequiredIdAsync<TEntity>(string? publicId, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
        => await GetIdByPublicIdAsync<TEntity>(publicId, cancellationToken)
           ?? throw new KeyNotFoundException($"{typeof(TEntity).Name} with public id '{publicId}' not found.");
}
