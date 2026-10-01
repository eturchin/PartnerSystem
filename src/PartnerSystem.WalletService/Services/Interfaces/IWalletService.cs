using PartnerSystem.Contracts.Dtos.Wallets;

namespace PartnerSystem.WalletService.Services.Interfaces;

public interface IWalletService
{
    Task<WalletBalanceDto> GetBalanceAsync(long userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<WalletPaymentDto>> GetPaymentsAsync(long userId, CancellationToken cancellationToken);
    Task<decimal> PayoutAsync(long userId, CancellationToken cancellationToken);
}