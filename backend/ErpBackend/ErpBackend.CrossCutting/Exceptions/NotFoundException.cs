using System.Net;
using ErpBackend.CrossCutting.Constants;

namespace ErpBackend.CrossCutting.Exceptions;

/// <summary>
/// Thrown when a requested resource does not exist. Maps to HTTP 404.
/// </summary>
public class NotFoundException : AppException
{
    public NotFoundException(string message = "")
        : base(string.IsNullOrWhiteSpace(message) ? ErrorMessages.NotFound : message,
               HttpStatusCode.NotFound, "NOT_FOUND")
    {
    }

    public NotFoundException(string entity, object key)
        : base(ErrorMessages.NotFoundWith(entity, key), HttpStatusCode.NotFound, "NOT_FOUND")
    {
    }
}
