using Competition.Application.Common.Models;
using Competition.Domain.Entities.FormationEntities;

namespace Competition.Application.Interfaces;

public interface IFormationMark : IRepository<FormationMark>
{
    Task<FormationMark?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<PagedResult<FormationMark>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default);
}

public interface IFormation : IRepository<Formation>
{
    Task<Formation?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<Formation?> GetByMarkIdAsync(Guid formationMarkId, CancellationToken cancellationToken = default);
    Task<PagedResult<Formation>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default);
}

public interface IPosition : IRepository<Position>
{
    Task<Position?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<bool> HasSubPositionsAsync(Guid positionId, CancellationToken cancellationToken = default);
    Task<bool> HasPlayersAsync(Guid positionId, CancellationToken cancellationToken = default);
    Task<PagedResult<Position>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default);
}

public interface ISubPosition : IRepository<SubPosition>
{
    Task<SubPosition?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<SubPosition?> GetWithPositionAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> HasPlayersAsync(Guid subPositionId, CancellationToken cancellationToken = default);
    Task<PagedResult<SubPosition>> GetPagedAsync(int page, int pageSize, string? search, Guid? positionId, CancellationToken cancellationToken = default);
}
