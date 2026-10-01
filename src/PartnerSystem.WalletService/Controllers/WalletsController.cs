using Microsoft.AspNetCore.Mvc;
using PartnerSystem.Contracts.Dtos.Wallets;
using PartnerSystem.WalletService.Models;
using PartnerSystem.WalletService.Services.Interfaces;

namespace PartnerSystem.WalletService.Controllers;

[ApiController]
[Route("api/wallets")]
public sealed class WalletsController(
    IWalletService walletService
) : ControllerBase
{
    [HttpGet("{userId:long}/balance")]
    [ProducesResponseType(typeof(WalletBalanceDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBalance(long userId, CancellationToken cancellationToken)
    {
        var balance = await walletService.GetBalanceAsync(userId, cancellationToken);
        
        return Ok(balance);
    }

    [HttpGet("{userId:long}/payments")]
    [ProducesResponseType(typeof(IReadOnlyList<WalletPaymentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayments(long userId, CancellationToken cancellationToken)
    {
        var payments = await walletService.GetPaymentsAsync(userId, cancellationToken);
        
        return Ok(payments);
    }

    [HttpPost("{userId:long}/payout")]
    [ProducesResponseType(typeof(PayoutResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Payout(long userId, CancellationToken cancellationToken)
    {
        var amount = await walletService.PayoutAsync(userId, cancellationToken);
        
        return Ok(new PayoutResponse(amount));
    }
}