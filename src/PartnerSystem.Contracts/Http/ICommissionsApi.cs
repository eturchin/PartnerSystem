using PartnerSystem.Contracts.Dtos.Commissions;
using Refit;

namespace PartnerSystem.Contracts.Http;

/// <summary>
/// Refit contract for PartnerSystem.CommissionService.
/// </summary>
public interface ICommissionsApi
{
    [Get("/api/commissions/by-event/{eventId}")]
    Task<IReadOnlyList<CommissionDetailDto>> GetByEventAsync(long eventId, CancellationToken cancellationToken);

    [Get("/api/commissions/by-user/{userId}")]
    Task<IReadOnlyList<CommissionDetailDto>> GetByUserAsync(long userId, CancellationToken cancellationToken);

    [Get("/api/commissions/schema")]
    Task<SchemaResponse> GetSchemaAsync(CancellationToken cancellationToken);

    [Put("/api/commissions/schema")]
    Task<SchemaResponse> SetSchemaAsync([Body] SchemaSwitchDto dto, CancellationToken cancellationToken);

    [Post("/api/commissions/mark-paid")]
    Task<MarkPaidResponse> MarkPaidAsync([Body] IReadOnlyCollection<long> commissionIds, CancellationToken cancellationToken);

    [Get("/api/commissions/unpaid/{userId}")]
    Task<IReadOnlyList<UnpaidCommissionDto>> GetUnpaidAsync(long userId, CancellationToken cancellationToken);
}