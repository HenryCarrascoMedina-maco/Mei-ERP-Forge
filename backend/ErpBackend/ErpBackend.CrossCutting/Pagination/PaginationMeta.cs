namespace ErpBackend.CrossCutting.Pagination;

/// <summary>
/// Pagination metadata describing the current page within a result set.
/// </summary>
public class PaginationMeta
{
    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalItems { get; set; }

    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);

    public bool HasPrevious => Page > 1;

    public bool HasNext => Page < TotalPages;

    public PaginationMeta() { }

    public PaginationMeta(int page, int pageSize, int totalItems)
    {
        Page = page;
        PageSize = pageSize;
        TotalItems = totalItems;
    }
}
