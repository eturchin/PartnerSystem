using PartnerSystem.Shared.Contracts.Enums;

namespace PartnerSystem.CommissionService.Services.Interfaces;

public interface ICommissionSchemaService
{
    Task<CommissionSchema> GetCurrentAsync(CancellationToken cancellationToken);
    Task<CommissionSchema> SetAsync(CommissionSchema schema, CancellationToken cancellationToken);
}
