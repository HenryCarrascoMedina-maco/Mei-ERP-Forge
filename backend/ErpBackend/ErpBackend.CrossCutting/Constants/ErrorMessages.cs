namespace ErpBackend.CrossCutting.Constants;

/// <summary>
/// Reusable, centralized error messages.
/// </summary>
public static class ErrorMessages
{
    public const string Unexpected = "An unexpected error occurred. Please try again later.";
    public const string NotFound = "The requested resource was not found.";
    public const string Unauthorized = "You are not authenticated.";
    public const string Forbidden = "You do not have permission to perform this action.";
    public const string ValidationFailed = "One or more validation errors occurred.";
    public const string Conflict = "The resource already exists or conflicts with existing data.";
    public const string BusinessRule = "The operation violates a business rule.";

    public static string NotFoundWith(string entity, object key) =>
        $"{entity} with identifier '{key}' was not found.";
}
