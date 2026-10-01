using Microsoft.EntityFrameworkCore;
using PartnerSystem.EventService.Data.Entities;
using PartnerSystem.Shared.Seeding;

namespace PartnerSystem.EventService.Data;

public class EventDbContext(DbContextOptions<EventDbContext> options) : DbContext(options)
{
    public DbSet<EventEntity> Events => Set<EventEntity>();
    public DbSet<OutboxMessageEntity> Outbox => Set<OutboxMessageEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ExternalId).IsUnique();
        });
        modelBuilder.Entity<OutboxMessageEntity>(e => e.HasKey(x => x.Id));
        
        modelBuilder.Entity<EventEntity>().HasData(
            new EventEntity
            {
                Id = SeedEvents.EventEve1000Pk,
                ExternalId = SeedEvents.EventEve1000Id,
                UserExternalId = SeedUsers.Eve,
                Profit = 1000m,
                OccurredAt = SeedEvents.BaseTime
            },
            new EventEntity
            {
                Id = SeedEvents.EventFrank500Pk,
                ExternalId = SeedEvents.EventFrank500Id,
                UserExternalId = SeedUsers.Frank,
                Profit = 500m,
                OccurredAt = SeedEvents.BaseTime.AddMinutes(1)
            }
        );
    }
}