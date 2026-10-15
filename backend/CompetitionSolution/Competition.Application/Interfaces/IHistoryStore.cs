using Competition.Application.Common.Models;
using Competition.Domain.Entities;

namespace Competition.Application.Interfaces;

public interface IHistoryStore
{
    Task<TransferValueHistory?> GetTransferAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<TransferValueHistory>> GetTransfersAsync(Guid? playerId, int page, int pageSize, CancellationToken cancellationToken);
    Task<PlayerSelectionHistory?> GetPlayerSelectionAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<PlayerSelectionHistory>> GetPlayerSelectionsAsync(Guid? playerId, Guid? selectionId, int page, int pageSize, CancellationToken cancellationToken);
    Task<SelectionManagerHistory?> GetManagerSelectionAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<SelectionManagerHistory>> GetManagerSelectionsAsync(Guid? managerId, Guid? selectionId, int page, int pageSize, CancellationToken cancellationToken);
    Task<MandateDate?> GetMandateAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<MandateDate>> GetMandatesAsync(Guid? playerId, Guid? managerId, Guid? selectionId, Guid? playerSelectionHistoryId, Guid? selectionManagerHistoryId, int page, int pageSize, CancellationToken cancellationToken);
    Task AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken) where TEntity : class;
    void Remove<TEntity>(TEntity entity) where TEntity : class;
    Task<bool> PlayerExistsAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ManagerExistsAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> SelectionExistsAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> PlayerSelectionHistoryExistsAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> SelectionManagerHistoryExistsAsync(Guid id, CancellationToken cancellationToken);
}
