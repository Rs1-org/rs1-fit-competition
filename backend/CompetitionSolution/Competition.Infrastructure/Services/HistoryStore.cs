using Competition.Application.Common.Models;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Competition.Infrastructure.Services;

public class HistoryStore(AppDbContext db) : IHistoryStore
{
    public Task<TransferValueHistory?> GetTransferAsync(Guid id, CancellationToken cancellationToken) =>
        db.TransferValueHistories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<PagedResult<TransferValueHistory>> GetTransfersAsync(Guid? playerId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = db.TransferValueHistories.AsQueryable();
        if (playerId is not null) query = query.Where(x => x.PlayerId == playerId);
        return query.OrderByDescending(x => x.CreatedAt).ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public Task<PlayerSelectionHistory?> GetPlayerSelectionAsync(Guid id, CancellationToken cancellationToken) =>
        db.PlayerSelectionHistories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<PagedResult<PlayerSelectionHistory>> GetPlayerSelectionsAsync(Guid? playerId, Guid? selectionId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = db.PlayerSelectionHistories.AsQueryable();
        if (playerId is not null) query = query.Where(x => x.PlayerId == playerId);
        if (selectionId is not null) query = query.Where(x => x.SelectionId == selectionId);
        return query.OrderByDescending(x => x.CreatedAt).ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public Task<SelectionManagerHistory?> GetManagerSelectionAsync(Guid id, CancellationToken cancellationToken) =>
        db.SelectionManagerHistories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<PagedResult<SelectionManagerHistory>> GetManagerSelectionsAsync(Guid? managerId, Guid? selectionId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = db.SelectionManagerHistories.AsQueryable();
        if (managerId is not null) query = query.Where(x => x.ManagerId == managerId);
        if (selectionId is not null) query = query.Where(x => x.SelectionId == selectionId);
        return query.OrderByDescending(x => x.CreatedAt).ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public Task<MandateDate?> GetMandateAsync(Guid id, CancellationToken cancellationToken) =>
        db.MandateDates.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<PagedResult<MandateDate>> GetMandatesAsync(Guid? playerId, Guid? managerId, Guid? selectionId, Guid? playerSelectionHistoryId, Guid? selectionManagerHistoryId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = db.MandateDates.AsQueryable();
        if (playerId is not null)
            query = query.Where(x => x.PlayerSelectionHistory != null && x.PlayerSelectionHistory.PlayerId == playerId);
        if (managerId is not null)
            query = query.Where(x => x.SelectionManagerHistory != null && x.SelectionManagerHistory.ManagerId == managerId);
        if (selectionId is not null)
            query = query.Where(x =>
                (x.PlayerSelectionHistory != null && x.PlayerSelectionHistory.SelectionId == selectionId)
                || (x.SelectionManagerHistory != null && x.SelectionManagerHistory.SelectionId == selectionId));
        if (playerSelectionHistoryId is not null)
            query = query.Where(x => x.PlayerSelectionHistoryId == playerSelectionHistoryId);
        if (selectionManagerHistoryId is not null)
            query = query.Where(x => x.SelectionManagerHistoryId == selectionManagerHistoryId);
        return query.OrderByDescending(x => x.CreatedAt).ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public async Task AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken) where TEntity : class =>
        await db.Set<TEntity>().AddAsync(entity, cancellationToken);

    public void Remove<TEntity>(TEntity entity) where TEntity : class =>
        db.Set<TEntity>().Remove(entity);

    public Task<bool> PlayerExistsAsync(Guid id, CancellationToken cancellationToken) =>
        db.Players.AnyAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);

    public Task<bool> ManagerExistsAsync(Guid id, CancellationToken cancellationToken) =>
        db.Managers.AnyAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);

    public Task<bool> SelectionExistsAsync(Guid id, CancellationToken cancellationToken) =>
        db.Selections.AnyAsync(x => x.Id == id, cancellationToken);

    public Task<bool> PlayerSelectionHistoryExistsAsync(Guid id, CancellationToken cancellationToken) =>
        db.PlayerSelectionHistories.AnyAsync(x => x.Id == id, cancellationToken);

    public Task<bool> SelectionManagerHistoryExistsAsync(Guid id, CancellationToken cancellationToken) =>
        db.SelectionManagerHistories.AnyAsync(x => x.Id == id, cancellationToken);
}
