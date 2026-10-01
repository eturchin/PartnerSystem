namespace PartnerSystem.Shared.Contracts.Events;

public sealed record ProfitEventMessage(
    long EventId,
    long UserExternalId,
    decimal Profit,
    DateTime OccurredAt);