namespace ErpBackend.CrossCutting.Pagination;

/// <summary>
/// Sorting direction used by list endpoints.
/// </summary>
public enum SortDirection
{
    Ascending,
    Descending
}

/// <summary>
/// Sorting request parameters.
/// </summary>
public class SortParams
{
    /// <summary>Property name to sort by (case-insensitive).</summary>
    public string? SortBy { get; set; }

    public SortDirection SortDirection { get; set; } = SortDirection.Ascending;

    public bool IsDescending => SortDirection == SortDirection.Descending;
}
