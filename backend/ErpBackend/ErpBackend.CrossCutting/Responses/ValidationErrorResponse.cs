namespace ErpBackend.CrossCutting.Responses;

/// <summary>
/// Error response that breaks validation failures down by field.
/// </summary>
public class ValidationErrorResponse : ErrorResponse
{
    /// <summary>Map of field name to the list of validation messages for that field.</summary>
    public IDictionary<string, string[]> Errors { get; set; } =
        new Dictionary<string, string[]>();
}
