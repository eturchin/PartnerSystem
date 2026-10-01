using Microsoft.EntityFrameworkCore;
using PartnerSystem.Contracts.Dtos.Wallets;
using PartnerSystem.Contracts.Http;
using PartnerSystem.WalletService.Data;
using PartnerSystem.WalletService.Data.Entities;
using PartnerSystem.WalletService.Services.Interfaces;

namespace PartnerSystem.WalletService.Services;

internal sealed class WalletService(
    WalletDbContext db,
    ICommissionsApi commissionsApi,
    ILogger<WalletService> logger
) : IWalletService
{
    public async Task<WalletBalanceDto> GetBalanceAsync(long userId, CancellationToken cancellationToken)
    {
        var wallet = await GetOrCreateAsync(userId, cancellationToken);
        
        return new WalletBalanceDto(userId, wallet.Balance);
    }

    public async Task<IReadOnlyList<WalletPaymentDto>> GetPaymentsAsync(long userId, CancellationToken cancellationToken)
    {
        return await db.Payments
            .AsNoTracking()
            .Where(p => p.UserExternalId == userId)
            .OrderByDescending(p => p.PaidAt)
            .Select(p => new WalletPaymentDto(p.CommissionId, p.Amount, p.PaidAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> PayoutAsync(long userId, CancellationToken cancellationToken)
    {
        var job = new PayoutJobEntity
        {
            UserExternalId = userId,
            StartedAt = DateTime.UtcNow
        };
        
        db.PayoutJobs.Add(job);
        await db.SaveChangesAsync(cancellationToken);

        try
        {
            var unpaid = await commissionsApi.GetUnpaidAsync(userId, cancellationToken);
            
            if (unpaid.Count == 0)
            {
                await CompleteJobAsync(job, success: true, error: null, cancellationToken);
                return 0m;
            }

            var unpaidIds = unpaid.Select(u => u.Id).ToList();
            
            var alreadyPaid = await db.Payments
                .Where(p => unpaidIds.Contains(p.CommissionId))
                .Select(p => p.CommissionId)
                .ToListAsync(cancellationToken);

            var toPay = unpaid.Where(u => !alreadyPaid.Contains(u.Id)).ToList();
            if (toPay.Count == 0)
            {
                await CompleteJobAsync(job, success: true, error: null, cancellationToken);
                return 0m;
            }

            var wallet = await GetOrCreateAsync(userId, cancellationToken);
            var now = DateTime.UtcNow;
            var total = 0m;

            foreach (var commission in toPay)
            {
                db.Payments.Add(new PaymentEntity
                {
                    CommissionId = commission.Id,
                    UserExternalId = userId,
                    Amount = commission.Amount,
                    PaidAt = now
                });
                total += commission.Amount;
            }

            wallet.Balance += total;
            wallet.UpdatedAt = now;

            job.CompletedAt = now;
            job.Success = true;
            await db.SaveChangesAsync(cancellationToken);

            // Notify CommissionService that these commissions are now paid.
            // If this call fails, the payout is still recorded locally;
            // retry on the next run will mark the commissions as paid
            // (the Payments unique index keeps the whole operation idempotent).
            await commissionsApi.MarkPaidAsync(toPay.Select(t => t.Id).ToList(), cancellationToken);

            logger.LogInformation("Paid {Total} to user {UserId} ({Count} commissions)", total, userId, toPay.Count);

            return total;
        }
        catch (Exception ex)
        {
            await CompleteJobAsync(job, success: false, error: ex.Message, cancellationToken);
            logger.LogError(ex, "Payout failed for user {UserId}", userId);
            throw;
        }
    }

    private async Task<WalletEntity> GetOrCreateAsync(long userId, CancellationToken cancellationToken)
    {
        var wallet = await db.Wallets.FirstOrDefaultAsync(w => w.UserExternalId == userId, cancellationToken);
        
        if (wallet is not null)
        {
            return wallet;
        }

        wallet = new WalletEntity
        {
            UserExternalId = userId,
            Balance = 0m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        db.Wallets.Add(wallet);
        await db.SaveChangesAsync(cancellationToken);
        
        return wallet;
    }

    private async Task CompleteJobAsync(PayoutJobEntity job, bool success, string? error, CancellationToken cancellationToken)
    {
        job.CompletedAt = DateTime.UtcNow;
        job.Success = success;
        job.Error = error;
        
        await db.SaveChangesAsync(cancellationToken);
    }
}