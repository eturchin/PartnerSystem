using PartnerSystem.Contracts.Dtos.Commissions;
using PartnerSystem.Shared.Contracts.Enums;

namespace PartnerSystem.AdminService.Services.Interfaces;

public interface ISchemaService
{
    Task<SchemaResponse> SwitchSchemaAsync(CommissionSchema schema, CancellationToken cancellationToken);
    Task<SchemaResponse> GetSchemaAsync(CancellationToken cancellationToken);
}