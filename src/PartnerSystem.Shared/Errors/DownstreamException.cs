namespace PartnerSystem.Shared.Errors;

public sealed class DownstreamException : AppException
{
    public override int StatusCode { get; }
    public override string Code => "downstream_error";

    public DownstreamException(string service, int statusCode, ApiError? error)
        : base(error?.Message ?? $"Downstream service '{service}' returned {statusCode}.")
    {
        StatusCode = statusCode;
    }
}