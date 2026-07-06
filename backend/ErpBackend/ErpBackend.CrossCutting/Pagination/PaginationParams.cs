namespace ErpBackend.CrossCutting.Pagination;

/// <summary>
/// Standard query parameters for paginated, searchable and sortable list endpoints.
/// </summary>
public class PaginationParams : SortParams
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;
    private int _page = 1;

    /// <summary>1-based page number.</summary>
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    /// <summary>Number of items per page, capped at <see cref="MaxPageSize"/>.</summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value switch
        {
            < 1 => 10,
            > MaxPageSize => MaxPageSize,
            _ => value
        };
    }

    /// <summary>Free-text global search term.</summary>
    public string? Search { get; set; }

    public int Skip => (Page - 1) * PageSize;
}
