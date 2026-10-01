using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PartnerSystem.EventService.Data;
using PartnerSystem.EventService.Data.Entities;
using PartnerSystem.EventService.Services.Interfaces;
using PartnerSystem.Shared.Contracts.Dtos.Events;
using PartnerSystem.Shared.Contracts.Events;
using PartnerSystem.Shared.Kafka;
using PartnerSystem.Shared.Options;

namespace PartnerSystem.EventService.Services;

internal sealed class EventService(EventDbContext db) : IEventsService
{
    public async Task<bool> CreateAsync(EventDto dto, CancellationToken cancellationToken)
    {
        // Idempotency: if the event is already stored, return false (already exists).
        var exists = await db.Events.AnyAsync(e => e.ExternalId == dto.EventId, cancellationToken);
        
        if (exists)
        {
            return false;
        }

        var entity = new EventEntity
        {
            ExternalId = dto.EventId,
            UserExternalId = dto.UserExternalId,
            Profit = dto.Profit,
            OccurredAt = dto.OccurredAt
        };
        
        db.Events.Add(entity);

        var message = new ProfitEventMessage(dto.EventId, dto.UserExternalId, dto.Profit, dto.OccurredAt);

        db.Outbox.Add(new OutboxMessageEntity
        {
            Topic = KafkaTopics.ProfitEvents,
            Payload = JsonSerializer.Serialize(message, JsonOptions.SerializerOptions),
            CreatedAt = DateTime.UtcNow,
            Published = false
        });

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<EventDto>> GetByUserAsync(long userExternalId, CancellationToken cancellationToken) =>
        await db.Events.
            AsNoTracking()
            .Where(e => e.UserExternalId == userExternalId)
            .OrderByDescending(e => e.OccurredAt)
            .Select(e => new EventDto(e.ExternalId, e.UserExternalId, e.Profit, e.OccurredAt))
            .ToListAsync(cancellationToken);

    public async Task<EventDto?> GetAsync(long eventId, CancellationToken cancellationToken)
    {
        var e = await db.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ExternalId == eventId, cancellationToken);
        
        return e is null ? null : new EventDto(e.ExternalId, e.UserExternalId, e.Profit, e.OccurredAt);
    }
}