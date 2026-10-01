using Microsoft.Extensions.Logging;
using PartnerSystem.Shared.Errors;

namespace PartnerSystem.Shared.Extensions;

public sealed class RefitExceptionHandler(ILogger<RefitExceptionHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return response;
        }
        
        logger.LogWarning(
            "Downstream call {Method} {Uri} failed with {StatusCode}",
            request.Method, request.RequestUri, (int)response.StatusCode);

        throw new DownstreamException(
            service: request.RequestUri?.Host ?? "unknown",
            statusCode: (int)response.StatusCode,
            error: null);
    }
}
