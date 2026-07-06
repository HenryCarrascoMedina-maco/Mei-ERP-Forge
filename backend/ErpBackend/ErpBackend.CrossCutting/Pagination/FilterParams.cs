namespace ErpBackend.CrossCutting.Pagination;

/// <summary>
/// Base filter model. Concrete modules derive from this to add typed filter properties
/// while inheriting pagination, search and sorting behavior.
/// </summary>
public abstract class FilterParams : PaginationParams
{
    /// <summary>Optional active/inactive filter. Null means "all".</summary>
    public bool? IsActive { get; set; }

    /// <summary>Optional lower bound for created-at filtering.</summary>
    public DateTime? CreatedFrom { get; set; }

    /// <summary>Optional upper bound for created-at filtering.</summary>
    public DateTime? CreatedTo { get; set; }
}
