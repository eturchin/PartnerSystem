using PartnerSystem.AdminService.Services;
using PartnerSystem.AdminService.Services.Interfaces;
using PartnerSystem.Contracts.Http;
using PartnerSystem.Shared.Extensions;

namespace PartnerSystem.AdminService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterAppServices(this IServiceCollection services)
    {
        services.AddScoped<ISchemaService, SchemaService>();
        services.AddScoped<IEventService, EventService>();
        
        return services;
    }

    public static IServiceCollection AddUpstreamClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPartnerRefitClient<ICommissionsApi>(configuration, "Services:CommissionService:BaseUrl");
        services.AddPartnerRefitClient<IEventsApi>(configuration, "Services:EventService:BaseUrl");

        return services;
    }
}