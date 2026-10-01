using Microsoft.EntityFrameworkCore;
using PartnerSystem.EventService.Data;

namespace PartnerSystem.EventService.Services;

public class OutboxPublisher(
    IServiceScopeFactory scopeFactory,
    KafkaProducer producer,
    ILogger<OutboxPublisher> logger
) : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();

                var pending = await db.Outbox
                    .Where(m => !m.Published)
                    .OrderBy(m => m.CreatedAt)
                    .Take(50)
                    .ToListAsync(stoppingToken);

                foreach (var msg in pending)
                {
                    await producer.PublishAsync(msg.Topic, msg.Id.ToString(), msg.Payload, stoppingToken);
                    msg.Published = true;
                }

                if (pending.Count > 0)
                {
                    await db.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка публикации из Outbox");
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }
}