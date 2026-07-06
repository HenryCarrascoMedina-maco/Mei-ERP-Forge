using System.Net;
using ErpBackend.CrossCutting.Constants;

namespace ErpBackend.CrossCutting.Exceptions;

/// <summary>
/// Thrown when an authenticated user lacks permission for an action. Maps to HTTP 403.
/// </summary>
public class ForbiddenException : AppException
{
    public ForbiddenException(string message = "")
        : base(string.IsNullOrWhiteSpace(message) ? ErrorMessages.Forbidden : message,
               HttpStatusCode.Forbidden, "FORBIDDEN")
    {
    }
}
