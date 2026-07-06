using System.Net;
using ErpBackend.CrossCutting.Constants;

namespace ErpBackend.CrossCutting.Exceptions;

/// <summary>
/// Thrown when a business rule is violated. Maps to HTTP 400.
/// </summary>
public class BusinessException : AppException
{
    public BusinessException(string message = "", string? errorCode = null)
        : base(string.IsNullOrWhiteSpace(message) ? ErrorMessages.BusinessRule : message,
               HttpStatusCode.BadRequest, errorCode ?? "BUSINESS_RULE")
    {
    }
}
