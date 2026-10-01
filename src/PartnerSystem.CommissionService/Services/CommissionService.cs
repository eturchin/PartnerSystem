using Microsoft.EntityFrameworkCore;
using PartnerSystem.CommissionService.Data;
using PartnerSystem.CommissionService.Services.Interfaces;
using PartnerSystem.Contracts.Dtos.Commissions;
using PartnerSystem.Shared.Contracts.Enums;

namespace PartnerSystem.CommissionService.Services;

internal class CommissionService(
    CommissionDbContext db,
    ILogger<CommissionService> logger
) : ICommissionService
{
    public async Task<IReadOnlyList<CommissionDetailDto>> GetByEventAsync(
        long eventId, CancellationToken cancellationToken)
    {
        return await db.Commissions.AsNoTracking()
            .Where(c => c.EventId == eventId)
            .OrderBy(c => c.Level)
            .Select(c => new CommissionDetailDto(
                c.Id, c.BeneficiaryUserId, c.Amount, c.Level,
                c.SchemaType, c.Status, c.AccruedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CommissionDetailDto>> GetByUserAsync(long userId, CancellationToken cancellationToken)
    {
        return await db.Commissions.AsNoTracking()
            .Where(c => c.BeneficiaryUserId == userId)
            .OrderByDescending(c => c.AccruedAt)
            .Select(c => new CommissionDetailDto(
                c.Id, c.BeneficiaryUserId, c.Amount, c.Level,
                c.SchemaType, c.Status, c.AccruedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UnpaidCommissionDto>> GetUnpaidAsync(long userId, CancellationToken cancellationToken)
    {
        return await db.Commissions.AsNoTracking()
            .Where(c => c.BeneficiaryUserId == userId && c.Status == CommissionStatus.Accrued)
            .OrderBy(c => c.AccruedAt)
            .Select(c => new UnpaidCommissionDto(c.Id, c.Amount, c.AccruedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<int> MarkPaidAsync(IReadOnlyCollection<long> commissionIds, CancellationToken cancellationToken)
    {
        if (commissionIds.Count == 0)
        {
            return 0;
        }

        var items = await db.Commissions
            .Where(c => commissionIds.Contains(c.Id) && c.Status == CommissionStatus.Accrued)
            .ToListAsync(cancellationToken);

        if (items.Count == 0) return 0;

        var now = DateTime.UtcNow;
        foreach (var item in items)
        {
            item.Status = CommissionStatus.Paid;
            item.PaidAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Marked {Count} commissions as paid", items.Count);
        
        return items.Count;
    }
}