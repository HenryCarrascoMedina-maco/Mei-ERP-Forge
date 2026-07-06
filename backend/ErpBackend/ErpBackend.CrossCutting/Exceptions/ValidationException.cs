using System.Net;
using ErpBackend.CrossCutting.Constants;

namespace ErpBackend.CrossCutting.Exceptions;

/// <summary>
/// Thrown when input validation fails. Carries a per-field error map. Maps to HTTP 422.
/// </summary>
public class ValidationException : AppException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base(ErrorMessages.ValidationFailed, HttpStatusCode.UnprocessableEntity, "VALIDATION_ERROR")
    {
        Errors = errors;
    }

    public ValidationException(string field, string error)
        : this(new Dictionary<string, string[]> { [field] = new[] { error } })
    {
    }
}
