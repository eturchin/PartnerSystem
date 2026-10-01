using PartnerSystem.EventService.Services.Interfaces;

namespace PartnerSystem.EventService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterAppServices(this IServiceCollection services)
    {
        services.AddScoped<IEventsService, Services.EventService>();
        
        return services;
    }
}