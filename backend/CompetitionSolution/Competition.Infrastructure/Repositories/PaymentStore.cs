using Competition.Application.Payments;
using Competition.Domain.Entities;
using Competition.Domain.Entities.Payments;
using Competition.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Competition.Infrastructure.Repositories;

public class PaymentStore(AppDbContext db) : IPaymentStore
{
    public Task<ApplicationUser?> FindUserAsync(Guid userId, CancellationToken cancellationToken) =>
        db.Users.FirstOrDefaultAsync(x => x.Id == userId && x.IsActive, cancellationToken);

    public Task<PaymentTransaction?> FindBySessionAsync(string sessionId, CancellationToken cancellationToken) =>
        db.PaymentTransactions.Include(x => x.User).FirstOrDefaultAsync(x => x.StripeSessionId == sessionId, cancellationToken);

    public Task<PaymentTransaction?> GetLatestAsync(Guid userId, CancellationToken cancellationToken) =>
        db.PaymentTransactions.AsNoTracking().Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync(cancellationToken);

    public async Task AddAsync(PaymentTransaction payment, CancellationToken cancellationToken) =>
        await db.PaymentTransactions.AddAsync(payment, cancellationToken);
}

public class PremiumAccess(AppDbContext db, IClock clock) : IPremiumAccess
{
    public Task<bool> HasAccessAsync(Guid userId, CancellationToken cancellationToken)
    {
        var now = clock.GetCurrentInstant();
        return db.Users.AnyAsync(x => x.Id == userId && x.IsActive && x.PremiumUntil > now, cancellationToken);
    }
}
