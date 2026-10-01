using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PartnerSystem.CommissionService.Data;
using PartnerSystem.CommissionService.Data.Entities;
using PartnerSystem.CommissionService.Options;
using PartnerSystem.CommissionService.Services.Interfaces;
using PartnerSystem.Shared.Contracts.Enums;

namespace PartnerSystem.CommissionService.Services;

internal sealed class CommissionSchemaService(
    CommissionDbContext db,
    IOptions<CommissionSchemaOptions> options,
    ILogger<CommissionSchemaService> logger
) : ICommissionSchemaService
{
    private readonly CommissionSchemaOptions _options = options.Value;

    public async Task<CommissionSchema> GetCurrentAsync(CancellationToken cancellationToken)
    {
        var entity = await db.Schemas.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == CommissionSchemaEntity.SingletonId, cancellationToken);

        if (entity is not null)
        {
            return entity.Schema;
        }
        
        entity = new CommissionSchemaEntity
        {
            Id = CommissionSchemaEntity.SingletonId,
            Schema = _options.Default,
            UpdatedAt = DateTime.UtcNow
        };
        
        db.Schemas.Add(entity);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Initialized commission schema with default {Schema}", entity.Schema);
        }
        catch (DbUpdateException)
        {
            entity = await db.Schemas
                .AsNoTracking()
                .FirstAsync(s => s.Id == CommissionSchemaEntity.SingletonId, cancellationToken);
        }

        return entity.Schema;
    }

    public async Task<CommissionSchema> SetAsync(CommissionSchema schema, CancellationToken cancellationToken)
    {
        var entity = await db.Schemas
            .FirstOrDefaultAsync(s => s.Id == CommissionSchemaEntity.SingletonId, cancellationToken);
        
        if (entity is null)
        {
            entity = new CommissionSchemaEntity
            {
                Id = CommissionSchemaEntity.SingletonId,
                Schema = schema,
                UpdatedAt = DateTime.UtcNow
            };
            db.Schemas.Add(entity);
        }
        else
        {
            entity.Schema = schema;
            entity.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Commission schema switched to {Schema}", schema);

        return schema;
    }
}