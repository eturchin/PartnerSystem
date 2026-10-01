using Microsoft.EntityFrameworkCore;
using PartnerSystem.WalletService.Data;
using PartnerSystem.WalletService.Services.Interfaces;

namespace PartnerSystem.WalletService.Services;

internal sealed class PayoutBackgroundService(
    IServiceScopeFactory scopeFactory,
    IConfiguration config,
    ILogger<PayoutBackgroundService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalSeconds = int.TryParse(config["Payout:IntervalSeconds"], out var s) ? s : 5;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<WalletDbContext>();
                var logic = scope.ServiceProvider.GetRequiredService<IWalletService>();

                var userIds = await db.Wallets
                    .AsNoTracking()
                    .Select(w => w.UserExternalId)
                    .ToListAsync(stoppingToken);

                foreach (var userId in userIds)
                {
                    try
                    {
                        await logic.PayoutAsync(userId, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Payout failed for user {UserId}", userId);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Payout background service iteration failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
        }
    }
}