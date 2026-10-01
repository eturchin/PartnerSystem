namespace PartnerSystem.Shared.Contracts.Events;

public sealed record CommissionPaidMessage(
    Guid CommissionId,
    Guid UserId,
    decimal Amount,
    DateTime PaidAt);