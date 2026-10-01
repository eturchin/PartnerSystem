using PartnerSystem.UserService.Services;
using PartnerSystem.UserService.Services.Interfaces;

namespace PartnerSystem.UserService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterAppServices(this IServiceCollection services)
    {
        services.AddScoped<ITreeService, TreeService>();
        services.AddScoped<IUserService, Services.UserService>();
        
        return services;
    }
}