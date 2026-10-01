namespace PartnerSystem.Contracts.Dtos.Wallets;

public sealed record WalletPaymentDto(long CommissionId, decimal Amount, DateTime PaidAt);