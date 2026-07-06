using System.Net;
using ErpBackend.CrossCutting.Constants;

namespace ErpBackend.CrossCutting.Exceptions;

/// <summary>
/// Thrown when the user is not authenticated. Maps to HTTP 401.
/// </summary>
public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "")
        : base(string.IsNullOrWhiteSpace(message) ? ErrorMessages.Unauthorized : message,
               HttpStatusCode.Unauthorized, "UNAUTHORIZED")
    {
    }
}
