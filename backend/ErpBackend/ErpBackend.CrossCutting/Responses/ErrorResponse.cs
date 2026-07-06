namespace ErpBackend.CrossCutting.Responses;

/// <summary>
/// Standard error envelope returned by the global exception handler.
/// </summary>
public class ErrorResponse
{
    public bool Success { get; set; } = false;

    public string Message { get; set; } = string.Empty;

    public int StatusCode { get; set; }

    public string? ErrorCode { get; set; }

    public string? CorrelationId { get; set; }

    /// <summary>Optional technical detail, populated only in development environments.</summary>
    public string? Detail { get; set; }
}
