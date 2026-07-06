using System.Net;

namespace ErpBackend.CrossCutting.Exceptions;

/// <summary>
/// Base application exception. Carries an HTTP status code and an optional machine-readable
/// error code so the global exception handler can produce a consistent error response.
/// </summary>
public class AppException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public string? ErrorCode { get; }

    public AppException(
        string message,
        HttpStatusCode statusCode = HttpStatusCode.BadRequest,
        string? errorCode = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}
