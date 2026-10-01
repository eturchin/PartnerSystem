using PartnerSystem.Contracts.Dtos.Commissions;

namespace PartnerSystem.CommissionService.Services.Interfaces;

public interface ICommissionService
{
    Task<IReadOnlyList<CommissionDetailDto>> GetByEventAsync(long eventId, CancellationToken cancellationToken);
    Task<IReadOnlyList<CommissionDetailDto>> GetByUserAsync(long userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<UnpaidCommissionDto>> GetUnpaidAsync(long userId, CancellationToken cancellationToken);
    Task<int> MarkPaidAsync(IReadOnlyCollection<long> commissionIds, CancellationToken cancellationToken);
}
