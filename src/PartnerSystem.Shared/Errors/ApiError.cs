namespace PartnerSystem.Shared.Errors;

/// <summary>
/// Standard error response body returned by all services.
/// </summary>
public sealed record ApiError(
    string Code,
    string Message,
    string? TraceId = null,
    IDictionary<string, string[]>? Details = null);
