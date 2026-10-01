namespace PartnerSystem.Shared.Errors;

public abstract class AppException : Exception
{
    /// <summary>HTTP status code to return to the client.</summary>
    public abstract int StatusCode { get; }

    /// <summary>Stable machine-readable error code (e.g. "not_found", "conflict").</summary>
    public abstract string Code { get; }

    protected AppException(string message)
        : base(message) { }

    protected AppException(string message, Exception innerException)
        : base(message, innerException) { }
}
