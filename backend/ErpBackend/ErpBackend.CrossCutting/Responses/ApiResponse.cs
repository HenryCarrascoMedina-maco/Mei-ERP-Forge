namespace ErpBackend.CrossCutting.Responses;

/// <summary>
/// Standard envelope returned by every successful API endpoint.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; } = true;

    public string? Message { get; set; }

    public T? Data { get; set; }

    public string? CorrelationId { get; set; }

    public ApiResponse() { }

    public ApiResponse(T? data, string? message = null)
    {
        Data = data;
        Message = message;
    }

    public static ApiResponse<T> Ok(T? data, string? message = null) => new(data, message);
}

/// <summary>
/// Non-generic helper for responses that do not carry a typed payload.
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Ok(string? message = null) => new() { Message = message };
}
