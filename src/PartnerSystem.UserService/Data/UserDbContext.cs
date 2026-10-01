using Microsoft.EntityFrameworkCore;
using PartnerSystem.Shared.Seeding;
using PartnerSystem.UserService.Data.Entities;

namespace PartnerSystem.UserService.Data;

public class UserDbContext(
    DbContextOptions<UserDbContext> options
) : DbContext(options)
{
    public DbSet<UserEntity> Users => Set<UserEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ExternalId).IsUnique();
            e.Property(x => x.Name).HasMaxLength(200);
        });
        
        modelBuilder.Entity<UserEntity>().HasData(
            new UserEntity
            {
                Id = SeedUsers.AlicePk,
                ExternalId = SeedUsers.Alice,
                Name = "Alice",
                PartnerExternalId = null
            },
            new UserEntity
            {
                Id = SeedUsers.BobPk,
                ExternalId = SeedUsers.Bob,
                Name = "Bob",
                PartnerExternalId = SeedUsers.Alice
            },
            new UserEntity
            {
                Id = SeedUsers.CharliePk,
                ExternalId = SeedUsers.Charlie,
                Name = "Charlie",
                PartnerExternalId = SeedUsers.Bob
            },
            new UserEntity
            {
                Id = SeedUsers.DavePk,
                ExternalId = SeedUsers.Dave,
                Name = "Dave",
                PartnerExternalId = SeedUsers.Charlie
            },
            new UserEntity
            {
                Id = SeedUsers.EvePk,
                ExternalId = SeedUsers.Eve,
                Name = "Eve",
                PartnerExternalId = SeedUsers.Dave
            },
            new UserEntity
            {
                Id = SeedUsers.FrankPk,
                ExternalId = SeedUsers.Frank,
                Name = "Frank",
                PartnerExternalId = null
            }
        );
    }
}
