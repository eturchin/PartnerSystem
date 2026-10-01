using Microsoft.EntityFrameworkCore;
using PartnerSystem.Shared.Seeding;
using PartnerSystem.WalletService.Data.Entities;

namespace PartnerSystem.WalletService.Data;

public class WalletDbContext(
    DbContextOptions<WalletDbContext> options
) : DbContext(options)
{
    public DbSet<WalletEntity> Wallets => Set<WalletEntity>();
    public DbSet<PaymentEntity> Payments => Set<PaymentEntity>();
    public DbSet<PayoutJobEntity> PayoutJobs => Set<PayoutJobEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WalletEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.UserExternalId).IsUnique();
            e.Property(x => x.Balance).HasPrecision(18, 4);
        });

        modelBuilder.Entity<PaymentEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.CommissionId).IsUnique();
            e.Property(x => x.Amount).HasPrecision(18, 4);
        });

        modelBuilder.Entity<PayoutJobEntity>(e => e.HasKey(x => x.Id));
        
        // Wallets for all seeded users — empty balance
        modelBuilder.Entity<WalletEntity>().HasData(
            new WalletEntity
            {
                Id = 1,
                UserExternalId = SeedUsers.Alice,
                Balance = 0m,
                CreatedAt = SeedEvents.BaseTime,
                UpdatedAt = SeedEvents.BaseTime
            },
            new WalletEntity
            {
                Id = 2,
                UserExternalId = SeedUsers.Bob,
                Balance = 0m,
                CreatedAt = SeedEvents.BaseTime,
                UpdatedAt = SeedEvents.BaseTime
            },
            new WalletEntity
            {
                Id = 3,
                UserExternalId = SeedUsers.Charlie,
                Balance = 0m,
                CreatedAt = SeedEvents.BaseTime,
                UpdatedAt = SeedEvents.BaseTime
            },
            new WalletEntity
            {
                Id = 4,
                UserExternalId = SeedUsers.Dave,
                Balance = 0m,
                CreatedAt = SeedEvents.BaseTime,
                UpdatedAt = SeedEvents.BaseTime
            },
            new WalletEntity
            {
                Id = 5,
                UserExternalId = SeedUsers.Eve,
                Balance = 0m,
                CreatedAt = SeedEvents.BaseTime,
                UpdatedAt = SeedEvents.BaseTime
            },
            new WalletEntity
            {
                Id = 6,
                UserExternalId = SeedUsers.Frank,
                Balance = 0m,
                CreatedAt = SeedEvents.BaseTime,
                UpdatedAt = SeedEvents.BaseTime
            }
        );
    }
}