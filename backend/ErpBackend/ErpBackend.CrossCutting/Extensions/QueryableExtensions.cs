using System.Linq.Expressions;
using System.Reflection;
using ErpBackend.CrossCutting.Pagination;

namespace ErpBackend.CrossCutting.Extensions;

/// <summary>
/// Reusable sorting and pagination operators over <see cref="IQueryable{T}"/>.
/// These keep list endpoints free of repetitive paging boilerplate.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Applies dynamic ordering based on a property name. Falls back to the source order
    /// when the property does not exist on <typeparamref name="T"/>.
    /// </summary>
    public static IQueryable<T> ApplySort<T>(this IQueryable<T> query, SortParams sort)
    {
        if (string.IsNullOrWhiteSpace(sort.SortBy))
        {
            return query;
        }

        var property = typeof(T).GetProperty(
            sort.SortBy,
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (property is null)
        {
            return query;
        }

        var parameter = Expression.Parameter(typeof(T), "x");
        var propertyAccess = Expression.MakeMemberAccess(parameter, property);
        var orderByExpression = Expression.Lambda(propertyAccess, parameter);

        var methodName = sort.IsDescending ? "OrderByDescending" : "OrderBy";

        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            new[] { typeof(T), property.PropertyType },
            query.Expression,
            Expression.Quote(orderByExpression));

        return query.Provider.CreateQuery<T>(resultExpression);
    }

    /// <summary>
    /// Materializes a single page of results into a <see cref="PagedResult{T}"/>,
    /// computing the total count from the (already filtered) query.
    /// </summary>
    public static PagedResult<T> ToPagedResult<T>(this IQueryable<T> query, PaginationParams pagination)
    {
        var totalItems = query.Count();

        var items = query
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .ToList();

        return new PagedResult<T>(items, pagination.Page, pagination.PageSize, totalItems);
    }

    /// <summary>
    /// Convenience helper that applies sorting and then paginates in a single call.
    /// </summary>
    public static PagedResult<T> ApplyPagination<T>(this IQueryable<T> query, PaginationParams pagination)
        => query.ApplySort(pagination).ToPagedResult(pagination);
}
