namespace ErpBackend.CrossCutting.Pagination;

/// <summary>
/// Internal paginated result returned by repositories and application services
/// before being mapped to a transport-level <c>PagedResponse</c>.
/// </summary>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }

    public PaginationMeta Meta { get; }

    public PagedResult(IReadOnlyList<T> items, int page, int pageSize, int totalItems)
    {
        Items = items;
        Meta = new PaginationMeta(page, pageSize, totalItems);
    }

    public static PagedResult<T> Empty(int page, int pageSize) =>
        new(Array.Empty<T>(), page, pageSize, 0);
}
