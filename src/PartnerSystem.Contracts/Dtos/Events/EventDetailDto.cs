using PartnerSystem.Contracts.Dtos.Commissions;

namespace PartnerSystem.Contracts.Dtos.Events;

public sealed record EventDetailDto(
    long EventId,
    long UserExternalId,
    decimal Profit,
    DateTime OccurredAt,
    List<CommissionDetailDto> Commissions);