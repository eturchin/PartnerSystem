using PartnerSystem.Contracts.Http;
using PartnerSystem.Shared.Extensions;
using PartnerSystem.WalletService.Services;
using PartnerSystem.WalletService.Services.Interfaces;

namespace PartnerSystem.WalletService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterAppServices(this IServiceCollection services)
    {
        services.AddScoped<IWalletService, Services.WalletService>();
        
        services.AddHostedService<PayoutBackgroundService>();
        
        return services;
    }
    
    public static IServiceCollection AddUpstreamClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddPartnerRefitClient<ICommissionsApi>(configuration, "Services:CommissionService:BaseUrl");

        return services;
    }
}