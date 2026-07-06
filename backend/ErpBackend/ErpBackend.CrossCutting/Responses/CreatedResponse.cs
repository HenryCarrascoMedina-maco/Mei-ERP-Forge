namespace ErpBackend.CrossCutting.Responses;

/// <summary>
/// Standard response returned after a resource has been created. Exposes the new identifier.
/// </summary>
public class CreatedResponse<TKey>
{
    public bool Success { get; set; } = true;

    public string? Message { get; set; }

    public TKey Id { get; set; } = default!;

    public CreatedResponse() { }

    public CreatedResponse(TKey id, string? message = null)
    {
        Id = id;
        Message = message;
    }
}
