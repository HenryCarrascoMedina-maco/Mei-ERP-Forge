using System.Net;
using ErpBackend.CrossCutting.Constants;

namespace ErpBackend.CrossCutting.Exceptions;

/// <summary>
/// Thrown for duplicated or conflicting data. Maps to HTTP 409.
/// </summary>
public class ConflictException : AppException
{
    public ConflictException(string message = "")
        : base(string.IsNullOrWhiteSpace(message) ? ErrorMessages.Conflict : message,
               HttpStatusCode.Conflict, "CONFLICT")
    {
    }
}
