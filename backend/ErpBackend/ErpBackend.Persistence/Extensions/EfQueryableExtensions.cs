using ErpBackend.CrossCutting.Extensions;
using ErpBackend.CrossCutting.Pagination;
using Microsoft.EntityFrameworkCore;

namespace ErpBackend.Persistence.Extensions;

/// <summary>
/// Async pagination over EF Core queries. Mirrors <c>QueryableExtensions.ApplyPagination</c> from
/// CrossCutting (which is synchronous and persistence-agnostic) but executes the count and page
/// fetch asynchronously against the database. Reuses CrossCutting's <c>ApplySort</c>, whose
/// expression-based ordering EF Core can translate to SQL.
/// </summary>
public static class EfQueryableExtensions
{
    /// <summary>Applies sorting, then asynchronously materializes a single page plus the total count.</summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query, PaginationParams pagination, CancellationToken cancellationToken = default)
    {
        var sorted = query.ApplySort(pagination);

        var totalItems = await sorted.CountAsync(cancellationToken);

        var items = await sorted
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, pagination.Page, pagination.PageSize, totalItems);
    }
}
