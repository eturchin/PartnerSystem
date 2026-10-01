namespace PartnerSystem.Shared.Contracts.Dtos.Events;

public sealed record EventDto(
    long EventId,
    long UserExternalId,
    decimal Profit,
    DateTime OccurredAt);