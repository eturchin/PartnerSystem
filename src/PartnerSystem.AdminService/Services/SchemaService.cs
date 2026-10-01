using PartnerSystem.AdminService.Services.Interfaces;
using PartnerSystem.Contracts.Dtos.Commissions;
using PartnerSystem.Contracts.Http;
using PartnerSystem.Shared.Contracts.Enums;

namespace PartnerSystem.AdminService.Services;

internal sealed class SchemaService(
    ICommissionsApi commissionsApi
) : ISchemaService
{
    public async Task<SchemaResponse> SwitchSchemaAsync(CommissionSchema schema, CancellationToken cancellationToken) =>
        await commissionsApi.SetSchemaAsync(new SchemaSwitchDto(schema), cancellationToken);

    public async Task<SchemaResponse> GetSchemaAsync(CancellationToken cancellationToken) =>
        await commissionsApi.GetSchemaAsync(cancellationToken);
}