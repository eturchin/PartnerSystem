using Microsoft.EntityFrameworkCore;
using PartnerSystem.Contracts.Dtos.Users;
using PartnerSystem.Shared.Contracts.Dtos.Users;
using PartnerSystem.UserService.Data;
using PartnerSystem.UserService.Data.Entities;
using PartnerSystem.UserService.Services.Interfaces;

namespace PartnerSystem.UserService.Services;

internal sealed class UserService(
    UserDbContext db,
    ITreeService treeService
) : IUserService
{

    public async Task<UserDetailsResponse> CreateAsync(UserDto dto, CancellationToken cancellationToken)
    {
        if (await db.Users.AnyAsync(u => u.ExternalId == dto.ExternalId, cancellationToken))
        {
            throw new InvalidOperationException($"User '{dto.ExternalId}' already exists.");
        }

        var entity = new UserEntity
        {
            ExternalId = dto.ExternalId,
            Name = dto.Name
        };

        db.Users.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return new UserDetailsResponse(entity.ExternalId, entity.Name, entity.PartnerExternalId);
    }

    public async Task<UserDetailsResponse?> GetAsync(long externalId, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.ExternalId == externalId, cancellationToken);

        return user is null
            ? null
            : new UserDetailsResponse(user.ExternalId, user.Name, user.PartnerExternalId);
    }

    public async Task SetPartnerAsync(long externalId, PartnerLinkDto dto, CancellationToken cancellationToken)
    {
        if (externalId != dto.UserExternalId)
        {
            throw new ArgumentException("ExternalId in the route and body must match.");
        }

        var user = await db.Users.FirstOrDefaultAsync(u => u.ExternalId == externalId, cancellationToken)
                   ?? throw new KeyNotFoundException($"User '{externalId}' was not found.");

        if (dto.PartnerExternalId is {} partnerId)
        {
            if (partnerId == externalId)
            {
                throw new ArgumentException("A user cannot be their own partner.");
            }

            var partnerExists = await db.Users
                .AsNoTracking()
                .AnyAsync(u => u.ExternalId == partnerId, cancellationToken);
           
            if (!partnerExists)
            {
                throw new ArgumentException($"Partner '{partnerId}' was not found.");
            }

            if (await treeService.WouldCreateCycleAsync(externalId, partnerId, cancellationToken))
            {
                throw new ArgumentException("Setting this partner link would create a cycle.");
            }
        }

        user.PartnerExternalId = dto.PartnerExternalId;
        await db.SaveChangesAsync(cancellationToken);
    }
}