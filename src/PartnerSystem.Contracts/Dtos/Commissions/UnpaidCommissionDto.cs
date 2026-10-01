namespace PartnerSystem.Contracts.Dtos.Commissions;

public sealed record UnpaidCommissionDto(long Id, decimal Amount, DateTime AccruedAt);