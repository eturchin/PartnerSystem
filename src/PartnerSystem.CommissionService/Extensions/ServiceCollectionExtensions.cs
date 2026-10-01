using PartnerSystem.CommissionService.Services;
using PartnerSystem.CommissionService.Services.Interfaces;
using PartnerSystem.Contracts.Http;
using PartnerSystem.Shared.Extensions;

namespace PartnerSystem.CommissionService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterAppServices(this IServiceCollection services)
    {
        services.AddScoped<ICommissionService, Services.CommissionService>();
        services.AddScoped<ICommissionSchemaService, CommissionSchemaService>();

        return services;
    }

    public static IServiceCollection AddUpstreamClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPartnerRefitClient<IUsersApi>(configuration, "Services:UserService:BaseUrl");
        
        return services;
    }
}