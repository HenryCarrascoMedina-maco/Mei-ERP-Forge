using ErpBackend.CrossCutting.Extensions;
using ErpBackend.CrossCutting.Pagination;
using Xunit;

namespace ErpBackend.Tests;

public class QueryableExtensionsTests
{
    private sealed record Item(string Name, int Order);

    private static readonly IQueryable<Item> Data = new[]
    {
        new Item("Charlie", 3), new Item("Alice", 1), new Item("Bob", 2),
        new Item("Dave", 4), new Item("Eve", 5),
    }.AsQueryable();

    [Fact]
    public void ApplySort_SortsAscendingByProperty()
    {
        var sorted = Data.ApplySort(new SortParams { SortBy = "Name", SortDirection = SortDirection.Ascending }).ToList();
        Assert.Equal("Alice", sorted[0].Name);
    }

    [Fact]
    public void ApplySort_SortsDescendingByProperty()
    {
        var sorted = Data.ApplySort(new SortParams { SortBy = "Name", SortDirection = SortDirection.Descending }).ToList();
        Assert.Equal("Eve", sorted[0].Name);
    }

    [Fact]
    public void ApplyPagination_ReturnsTheRequestedPageAndTotal()
    {
        var page = Data.ApplyPagination(new PaginationParams { Page = 2, PageSize = 2, SortBy = "Order" });

        Assert.Equal(5, page.Meta.TotalItems);
        Assert.Equal(2, page.Meta.Page);
        Assert.Equal(2, page.Items.Count);
        Assert.Equal("Charlie", page.Items[0].Name); // items 3 & 4 on page 2 (size 2), ordered by Order
    }
}
