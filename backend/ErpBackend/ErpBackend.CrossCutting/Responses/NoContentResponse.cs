namespace ErpBackend.CrossCutting.Responses;

/// <summary>
/// Standard response for successful operations that return no data (e.g. update, delete).
/// </summary>
public class NoContentResponse
{
    public bool Success { get; set; } = true;

    public string? Message { get; set; }

    public NoContentResponse(string? message = null)
    {
        Message = message;
    }
}
