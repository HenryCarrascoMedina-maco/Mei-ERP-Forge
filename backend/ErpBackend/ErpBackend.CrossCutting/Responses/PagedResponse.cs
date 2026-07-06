using ErpBackend.CrossCutting.Pagination;

namespace ErpBackend.CrossCutting.Responses;

/// <summary>
/// Standard envelope for paginated list endpoints. Carries the page items and metadata.
/// </summary>
public class PagedResponse<T>
{
    public bool Success { get; set; } = true;

    public string? Message { get; set; }

    public IReadOnlyList<T> Data { get; set; } = Array.Empty<T>();

    public PaginationMeta Meta { get; set; } = new();

    public string? CorrelationId { get; set; }

    public PagedResponse() { }

    public PagedResponse(PagedResult<T> result, string? message = null)
    {
        Data = result.Items;
        Meta = result.Meta;
        Message = message;
    }

    public static PagedResponse<T> From(PagedResult<T> result, string? message = null) =>
        new(result, message);
}
