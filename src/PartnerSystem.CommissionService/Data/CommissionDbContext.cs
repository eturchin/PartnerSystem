using Microsoft.EntityFrameworkCore;
using PartnerSystem.CommissionService.Data.Entities;
using PartnerSystem.Shared.Contracts.Enums;
using PartnerSystem.Shared.Seeding;

namespace PartnerSystem.CommissionService.Data;

public class CommissionDbContext(
    DbContextOptions<CommissionDbContext> options
) : DbContext(options)
{
    public DbSet<CommissionEntity> Commissions => Set<CommissionEntity>();
    public DbSet<CommissionSchemaEntity> Schemas => Set<CommissionSchemaEntity>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CommissionEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.EventId, x.BeneficiaryUserId }).IsUnique();
        });
        
        modelBuilder.Entity<CommissionSchemaEntity>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.Schema)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();
        });
        
         modelBuilder.Entity<CommissionSchemaEntity>().HasData(
            new CommissionSchemaEntity
            {
                Id = CommissionSchemaEntity.SingletonId,
                Schema = CommissionSchema.Linear,
                UpdatedAt = SeedEvents.BaseTime
            }
        );

        // Commissions for EventEve1000 (Linear, profit=1000)
        //   L1 Dave=10, L2 Charlie=20, L3 Bob=30, L4 Alice=40
        modelBuilder.Entity<CommissionEntity>().HasData(
            new CommissionEntity
            {
                Id = 1,
                EventId = SeedEvents.EventEve1000Id,
                BeneficiaryUserId = SeedUsers.Dave,
                SourceUserId = SeedUsers.Eve,
                Amount = 10m,
                Level = 1,
                SchemaType = CommissionSchema.Linear,
                Status = CommissionStatus.Accrued,
                AccruedAt = SeedEvents.BaseTime,
                PaidAt = null
            },
            new CommissionEntity
            {
                Id = 2,
                EventId = SeedEvents.EventEve1000Id,
                BeneficiaryUserId = SeedUsers.Charlie,
                SourceUserId = SeedUsers.Eve,
                Amount = 20m,
                Level = 2,
                SchemaType = CommissionSchema.Linear,
                Status = CommissionStatus.Accrued,
                AccruedAt = SeedEvents.BaseTime,
                PaidAt = null
            },
            new CommissionEntity
            {
                Id = 3,
                EventId = SeedEvents.EventEve1000Id,
                BeneficiaryUserId = SeedUsers.Bob,
                SourceUserId = SeedUsers.Eve,
                Amount = 30m,
                Level = 3,
                SchemaType = CommissionSchema.Linear,
                Status = CommissionStatus.Accrued,
                AccruedAt = SeedEvents.BaseTime,
                PaidAt = null
            },
            new CommissionEntity
            {
                Id = 4,
                EventId = SeedEvents.EventEve1000Id,
                BeneficiaryUserId = SeedUsers.Alice,
                SourceUserId = SeedUsers.Eve,
                Amount = 40m,
                Level = 4,
                SchemaType = CommissionSchema.Linear,
                Status = CommissionStatus.Accrued,
                AccruedAt = SeedEvents.BaseTime,
                PaidAt = null
            }
        );
    }
}
