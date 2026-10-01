using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PartnerSystem.Shared.Options;
using Polly;
using Refit;

namespace PartnerSystem.Shared.Extensions;

public static class RefitServiceCollectionExtensions
{
    public static IServiceCollection AddPartnerRefitClient<TClient>(
        this IServiceCollection services,
        IConfiguration configuration,
        string baseUrlKey)
        where TClient : class
    {
        var baseUrl = configuration[baseUrlKey]
                      ?? throw new InvalidOperationException($"Configuration value '{baseUrlKey}' is not set.");

        var refitSettings = new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions.SerializerOptions)
        };

        services.AddTransient<RefitExceptionHandler>();

        services.AddRefitClient<TClient>(refitSettings)
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl))
            .AddHttpMessageHandler<RefitExceptionHandler>()
            .AddStandardResilienceHandler(options =>
            {
                // Retry
                options.Retry.MaxRetryAttempts = 3;
                options.Retry.BackoffType = DelayBackoffType.Exponential;
                options.Retry.UseJitter = true;
                options.Retry.Delay = TimeSpan.FromMilliseconds(200);

                // Circuit breaker
                options.CircuitBreaker.FailureRatio = 0.5;
                options.CircuitBreaker.MinimumThroughput = 10;
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
                options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(10);

                // Timeouts
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
            });

        return services;
    }
}